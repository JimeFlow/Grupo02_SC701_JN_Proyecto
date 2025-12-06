
/*******************************RESERVAS(PRESTAMOS)******************************************/

ALTER PROCEDURE ObtenerListaLibrosCliente
AS
BEGIN
SELECT 
    L.Id_Libro,
    L.Titulo,
    L.Autor,
    L.Imagen_URL,
    L.Descripcion,
	L.ISBN,
	L.Anio,
    COUNT(E.Id_Ejemplar) AS Disponibles
FROM Libro L
INNER JOIN Ejemplar E 
    ON L.Id_Libro = E.Id_Libro 
    AND E.Estado = 'Disponible'
GROUP BY 
    L.Id_Libro, L.Titulo, L.Autor, L.Imagen_URL, L.Descripcion, L.ISBN, L.Anio
ORDER BY L.Titulo;
END

EXEC ObtenerListaLibrosCliente
--*************************************

-- RESERVAR LIRBO -------------------------------------------------------------------------------------
CREATE PROCEDURE ReservarLibro
    @Id_Libro INT,--
    @Fecha DATETIME,
    @Fecha_Vencimiento DATETIME,
    @Id_Usuario INT
AS
BEGIN
    DECLARE @Id_Ejemplar INT;

	SELECT TOP 1 @Id_Ejemplar = Id_Ejemplar
	FROM Ejemplar WHERE Id_Libro = @Id_Libro 
	AND Estado = 'Disponible' ORDER BY Id_Ejemplar;

	IF @Id_Ejemplar IS NULL
	BEGIN 
		SELECT -1 AS Resultado
		RETURN;
	END

    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Estado, Id_Usuario, Id_Ejemplar)
    VALUES ('Préstamo', @Fecha, @Fecha_Vencimiento, 4, @Id_Usuario, @Id_Ejemplar);

	UPDATE Ejemplar SET Estado = 'Prestado' WHERE Id_Ejemplar = @Id_Ejemplar;

    SELECT 1 AS Resultado;
END;



ALTER PROCEDURE ObtenerReservasUsuario
    @Id_Usuario INT
AS
BEGIN
    SELECT 
        M.Id_Movimiento,
        M.Fecha,
        M.Fecha_Vencimiento,
		M.Id_Ejemplar,
        L.Id_Libro,
        L.Titulo,
        L.Imagen_URL,
        E.Estado
    FROM Movimiento M INNER JOIN Ejemplar E ON M.Id_Ejemplar = E.Id_Ejemplar
    INNER JOIN Libro L ON L.Id_Libro = E.Id_Libro
    INNER JOIN Estado S ON S.Id_Estado = M.Id_Estado
    WHERE M.Id_Usuario = @Id_Usuario
      AND M.Tipo = 'Préstamo'
      AND (M.Id_Estado = 4 OR M.Id_Estado = 5 OR M.Id_Estado = 7)
    ORDER BY M.Fecha DESC;
END

EXEC ObtenerReservasUsuario 8;

----------------------CAMBIAR ESTADO A RESERVADO (ADMIN)------------------------------------------

CREATE PROCEDURE CambiarEstadoReservaAdmin
	@Id_Movimiento INT
AS
BEGIN
	UPDATE Movimiento SET Id_Estado = 5 WHERE Id_Movimiento = @Id_Movimiento; 
END;

----------------------CANCELAR RESERVA (ESTADO)---------------------------------------------------
UPDATE Estado SET Estado = 'Cancelado' WHERE Id_Estado = 2;

ALTER PROCEDURE CancelarReserva
    @Id_Movimiento INT
AS
BEGIN

    UPDATE Movimiento
    SET Id_Estado = 2
    WHERE Id_Movimiento = @Id_Movimiento;

END;

---------------------------------- EXTENDER PLAZO DE PRESTAMO --------------------------------------

CREATE PROCEDURE ExtenderPrestamoCliente
	@Fecha DATETIME,
	@Fecha_Vencimiento DATETIME,
	@Id_Movimiento INT
AS
BEGIN
	UPDATE Movimiento SET Fecha = @Fecha, Fecha_Vencimiento = @Fecha_Vencimiento 
	WHERE Id_Movimiento = @Id_Movimiento;
END;

----------------CAMBIAR ESTADO DE CANCELADO A DISPONIBLE (ES DECIR, QUE SE DEVOLVIÓ CORRECTAMENTE---------

CREATE PROCEDURE CambiarEstadoCancelado
	@Id_Movimiento INT
AS
BEGIN
	UPDATE Movimiento SET Id_Estado = 7 WHERE Id_Movimiento = @Id_Movimiento; --COMPLETADO

	UPDATE Ejemplar
    SET Estado = 'Disponible'
    WHERE Id_Ejemplar = (SELECT Id_Ejemplar FROM Movimiento WHERE Id_Movimiento = @Id_Movimiento);
END;

/************************************SANCIONES*****************************************************/
DROP TABLE IF EXISTS Sancion;

CREATE TABLE Sancion(
    Id_Sancion INT IDENTITY(1,1) PRIMARY KEY,
    Id_Usuario INT NOT NULL,
    Estado BIT DEFAULT 1,  -- 1 = Activo, 0 = Expirado
    Fecha_Inicio DATETIME DEFAULT GETDATE(),
    Fecha_Finalizacion DATETIME DEFAULT DATEADD(DAY, 14, GETDATE()),
    FOREIGN KEY (Id_Usuario) REFERENCES Usuario(Id_Usuario)
);

CREATE PROCEDURE RegistrarSancion
    @Id_Usuario INT
AS
BEGIN
    INSERT INTO Sancion (Id_Usuario)
    VALUES (@Id_Usuario);

    SELECT SCOPE_IDENTITY() AS IdSancion;
END;

------------------------------------VALIDAR ATRASOS-----------------------------------------------
CREATE PROCEDURE ObtenerMovimientosAtrasados
AS
BEGIN
    SELECT M.Id_Movimiento, M.Id_Usuario
    FROM Movimiento M
    WHERE M.Id_Estado = 5
      AND DATEADD(DAY, 2, M.Fecha_Vencimiento) < GETDATE(); --DOS DIAS DE MARGEN SINO SANCION
END;

-------------------------------VERIFICAR SI USUARIO TIENE SANCION ACTIVA--------------------------
CREATE PROCEDURE UsuarioTieneSancionActiva
    @Id_Usuario INT
AS
BEGIN
    SELECT COUNT(*)
    FROM Sancion
    WHERE Id_Usuario = @Id_Usuario
      AND Estado = 1 
      AND Fecha_Finalizacion > GETDATE();
END;

----------------------------------VERIFICAR SI YA CUMPLIÓ SANCION ALGUN USUARIO--------------------
CREATE OR ALTER PROCEDURE VerificarSancionesCumplidas
AS
BEGIN
	SELECT Id_Usuario
    FROM Sancion 
    WHERE Estado = 1 
      AND Fecha_Finalizacion < GETDATE();
END;

----------------------------------------INACTIVAR SANCION A USUARIO----------------------------------

CREATE OR ALTER PROCEDURE InactivarSancionUsuario
	@Id_Usuario INT
AS
BEGIN
	UPDATE Sancion SET Estado = 0 WHERE Id_Usuario = @Id_Usuario AND Estado = 1;
END;



/***********************************OBTENER RESERVAS PENDIENTES************************************/

---------------METODO GENERAL DE RESERVAS PARA EL ADMIN--------------------------------------------

ALTER PROCEDURE ObtenerReservasAdmin
    @EstadoFiltro INT = NULL
AS
BEGIN
    SELECT 
        M.Id_Movimiento,
        M.Fecha,
        M.Fecha_Vencimiento,
        CONCAT(U.Nombre,' ',U.Apellidos) AS Nombre,
        U.Identificacion,
        E.Estado,
        M.Id_Estado,
        M.Id_Ejemplar
    FROM Movimiento M
    INNER JOIN Usuario U ON M.Id_Usuario = U.Id_Usuario
    INNER JOIN Estado E ON M.Id_Estado = E.Id_Estado
    WHERE (@EstadoFiltro IS NULL OR M.Id_Estado = @EstadoFiltro)
    ORDER BY M.Fecha DESC;
END;


----------------------------------------------------------------------------
CREATE PROCEDURE ObtenerReservasPendientesAdmin
AS
BEGIN
	SELECT Id_Movimiento, Fecha, Fecha_Vencimiento, CONCAT(U.Nombre,' ', U.Apellidos) AS Nombre, 
	U.Identificacion FROM Movimiento M INNER JOIN Usuario U 
	ON M.Id_Usuario = U.Id_Usuario WHERE Id_Estado = 4 --PENDIENTE
END;

EXEC ObtenerReservasPendientesAdmin;

CREATE PROCEDURE ObtenerTodasLasReservasAdmin
AS
BEGIN
	SELECT Id_Movimiento, Fecha, Fecha_Vencimiento, CONCAT(U.Nombre,' ', U.Apellidos) AS Nombre, 
	U.Identificacion FROM Movimiento M INNER JOIN Usuario U 
	ON M.Id_Usuario = U.Id_Usuario WHERE Id_Estado = 1 --ACTIVO
END;

CREATE PROCEDURE ObtenerTodasLasReservasGeneral
AS
BEGIN
	SELECT Id_Movimiento, Fecha, Fecha_Vencimiento, CONCAT(U.Nombre,' ', U.Apellidos) AS Nombre, 
	U.Identificacion, e.Estado FROM Movimiento M INNER JOIN Usuario U 
	ON M.Id_Usuario = U.Id_Usuario INNER JOIN Estado E ON M.Id_Estado = E.Id_Estado
END;

-------------------------------------------------------------------------------------------------------


ALTER PROCEDURE [dbo].[ObtenerEjemplares]
AS
BEGIN
    SELECT 
        E.Id_Ejemplar,
        E.CodigoEjemplar,
        E.Estado,
        E.Ubicacion,
        E.Fecha_Registro,
        E.Id_Libro,
        L.Titulo
    FROM Ejemplar E
    INNER JOIN Libro L ON L.Id_Libro = E.Id_Libro;
END

--------------------------------------------------------------------------------------
ALTER PROCEDURE [dbo].[ObtenerEjemplarPorId]
    @Id INT
AS
BEGIN
    SELECT 
        E.Id_Ejemplar,
        E.CodigoEjemplar,
        E.Estado,
        E.Ubicacion,
        E.Fecha_Registro,
        E.Id_Libro,
        L.Titulo
    FROM Ejemplar E
    INNER JOIN Libro L ON L.Id_Libro = E.Id_Libro
    WHERE E.Id_Ejemplar = @Id;
END

---------------------------------------------------------------------------------------------------

------------------------------------EJEMPLARES-----------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[ActualizarEjemplar]
    @Id INT,
    @CodigoEjemplar VARCHAR(20),
    @Estado VARCHAR(50),
    @Ubicacion VARCHAR(100),
    @Id_Libro INT
AS
BEGIN
    UPDATE Ejemplar SET 
        CodigoEjemplar = @CodigoEjemplar,
        Estado = @Estado,
        Ubicacion = @Ubicacion,
        Id_Libro = @Id_Libro
    WHERE Id_Ejemplar = @Id;
END

CREATE OR ALTER PROCEDURE [dbo].[RegistrarEjemplar]
    @CodigoEjemplar VARCHAR(20),
    @Id_Libro INT,
    @Estado VARCHAR(50),
    @Ubicacion VARCHAR(100)
AS
BEGIN
    INSERT INTO Ejemplar (CodigoEjemplar, Id_Libro, Estado, Ubicacion, Fecha_Registro)
    VALUES (@CodigoEjemplar, @Id_Libro, @Estado, @Ubicacion, GETDATE());
END

CREATE OR ALTER PROCEDURE [dbo].[ObtenerEjemplares]
AS
BEGIN
    SELECT 
        E.Id_Ejemplar,
        E.CodigoEjemplar,
        E.Estado,
        E.Ubicacion,
        E.Fecha_Registro,
        E.Id_Libro,
        L.Titulo
    FROM Ejemplar E
    INNER JOIN Libro L ON L.Id_Libro = E.Id_Libro;
END

