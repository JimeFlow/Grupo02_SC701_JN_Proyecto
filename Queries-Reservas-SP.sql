/*******************************LIBROS POR SI ACASO******************************************/
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

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[ActualizarLibro]    Script Date: 11/12/2025 23:53:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[ActualizarLibro]
	@Id_Usuario INT,
    @Id_Libro INT,
    @ISBN VARCHAR(20),
	@Descripcion VARCHAR(150),
    @Titulo VARCHAR(200),
    @Autor VARCHAR(150),
    @Anio INT,
    @Imagen_URL VARCHAR(600),
    @Id_Categoria INT
AS
BEGIN
    UPDATE Libro
    SET 
        ISBN = @ISBN,
        Id_Categoria = @Id_Categoria,
		Descripcion = @Descripcion,
        Titulo = @Titulo,
        Autor = @Autor,
        Anio = @Anio,
        Imagen_URL = @Imagen_URL
    WHERE Id_Libro = @Id_Libro;

	INSERT INTO Logs(Fecha, Tipo_Accion, Descripcion_Accion, Modulo_Afectado,Id_Usuario)
	VALUES (GETDATE(),'Actualización', CONCAT('Se actualizó el libro ', @Titulo), 'Libro', @Id_Usuario);

END;
GO

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[ObtenerCategorias]    Script Date: 11/12/2025 23:53:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[ObtenerCategorias]
AS
BEGIN
	SELECT Id_Categoria, Tipo FROM Categoria
END;
GO

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[ObtenerEjemplares]    Script Date: 11/12/2025 23:54:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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
GO

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[RegistrarCategoria]    Script Date: 11/12/2025 23:54:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[RegistrarCategoria]
@Tipo VARCHAR(75)
AS
BEGIN
	INSERT INTO Categoria(Tipo) VALUES
	(@Tipo)
END;
GO

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[RegistrarEjemplar]    Script Date: 11/12/2025 23:54:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


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
GO

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[RegistrarLibro]    Script Date: 11/12/2025 23:54:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[RegistrarLibro]
    @ISBN VARCHAR(20),
	@Descripcion VARCHAR(150),
    @Titulo VARCHAR(200),
    @Autor VARCHAR(150),
    @Anio INT,
    @Imagen_URL VARCHAR(600),
	@Id_Categoria INT
AS
BEGIN
    INSERT INTO Libro(ISBN, Titulo, Descripcion, Autor, Id_Categoria, Anio, Imagen_URL)
    VALUES (@ISBN, @Titulo, @Descripcion, @Autor, @Id_Categoria, @Anio, @Imagen_URL);
END;
GO

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[ReservarLibro]    Script Date: 11/12/2025 23:55:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[ReservarLibro]
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
GO


USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[ObtenerLibroPorId]    Script Date: 12/12/2025 02:07:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerLibroPorId]
    @Id_Libro INT
AS
BEGIN
    SELECT 
        Id_Libro,
        ISBN,
        Titulo,
		Descripcion,
		L.Id_Categoria,
		C.Tipo AS Tipo,
        Autor,
        Anio,
        Imagen_URL
    FROM Libro L INNER JOIN Categoria C ON L.Id_Categoria = C.Id_Categoria
    WHERE Id_Libro = @Id_Libro;
END;
GO
EXEC ObtenerLibroPorId 3;










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



CREATE OR ALTER PROCEDURE ObtenerReservasUsuario
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
        S.Estado
    FROM Movimiento M INNER JOIN Ejemplar E ON M.Id_Ejemplar = E.Id_Ejemplar
    INNER JOIN Libro L ON L.Id_Libro = E.Id_Libro
    INNER JOIN Estado S ON S.Id_Estado = M.Id_Estado
    WHERE M.Id_Usuario = @Id_Usuario
      AND M.Tipo = 'Préstamo'
      AND (M.Id_Estado = 4 OR M.Id_Estado = 5 OR M.Id_Estado = 7 OR M.Id_Estado = 2)
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

CREATE OR ALTER PROCEDURE CancelarReserva
    @Id_Movimiento INT
AS
BEGIN
	DECLARE @EjemplarId INT;	

	SELECT @EjemplarId = Id_Ejemplar FROM Movimiento WHERE Id_Movimiento = @Id_Movimiento;

    UPDATE Movimiento
    SET Id_Estado = 2
    WHERE Id_Movimiento = @Id_Movimiento;

	UPDATE Ejemplar SET Estado = 'Disponible' WHERE Id_Ejemplar = @EjemplarId;	

END;

---------------------------------- EXTENDER PLAZO DE PRESTAMO --------------------------------------

CREATE OR ALTER PROCEDURE ExtenderPrestamoCliente
	@Fecha_Vencimiento DATETIME,
	@Id_Movimiento INT
AS
BEGIN
	UPDATE Movimiento SET Fecha_Vencimiento = @Fecha_Vencimiento 
	WHERE Id_Movimiento = @Id_Movimiento;
END;

----------------CAMBIAR ESTADO DE CANCELADO A DISPONIBLE (ES DECIR, QUE SE DEVOLVIÓ CORRECTAMENTE---------

CREATE OR ALTER PROCEDURE CambiarEstadoCompletado
	@Id_Movimiento INT
AS
BEGIN
	DECLARE @FechaVenc DATETIME;
	DECLARE @Id_Usuario INT

	SELECT @FechaVenc = Fecha_Vencimiento, @Id_Usuario = Id_Usuario FROM Movimiento WHERE Id_Movimiento = @Id_Movimiento;

	UPDATE Movimiento SET Id_Estado = 7 WHERE Id_Movimiento = @Id_Movimiento; --COMPLETADO

	IF @FechaVenc < GETDATE()
	BEGIN
		INSERT INTO Sancion (Id_Usuario)
		VALUES (@Id_Usuario);
	END;

	UPDATE Ejemplar
    SET Estado = 'Disponible'
    WHERE Id_Ejemplar = (SELECT Id_Ejemplar FROM Movimiento WHERE Id_Movimiento = @Id_Movimiento);
	SELECT 1 AS Resultado;
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


-------------------------------VERIFICAR SI USUARIO TIENE SANCION ACTIVA--------------------------
CREATE OR ALTER PROCEDURE UsuarioTieneSancionActiva
    @Id_Usuario INT
AS
BEGIN
    SELECT COUNT(*)
    FROM Sancion
    WHERE Id_Usuario = @Id_Usuario
      AND Estado = 1 
      AND Fecha_Finalizacion > GETDATE();
END;
EXEC UsuarioTieneSancionActiva 1;
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


CREATE OR ALTER PROCEDURE ReservasPorVencerPronto
AS
BEGIN
	SELECT Id_Movimiento, Tipo, Fecha, Fecha_Vencimiento, M.Id_Usuario, U.Nombre, L.Titulo, U.Correo 
	FROM Movimiento M INNER JOIN Usuario U ON M.Id_Usuario = U.Id_Usuario
	INNER JOIN Ejemplar E ON M.Id_Ejemplar = E.Id_Ejemplar INNER JOIN Libro L
	ON E.Id_Libro = L.Id_Libro
	WHERE CONVERT(date, M.Fecha_Vencimiento) = CONVERT(date, DATEADD(DAY, 1, GETDATE())) AND M.Id_Estado = 5
END;
EXEC ReservasPorVencerPronto;
-------------------------------------------------------------------------------------------------------

USE [BiblioSolaris]
GO

/****** Object:  StoredProcedure [dbo].[ObtenerEjemplares]    Script Date: 12/12/2025 15:55:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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
GO



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

ALTER PROCEDURE [dbo].[ObtenerReservasUsuario]
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
        S.Estado
    FROM Movimiento M INNER JOIN Ejemplar E ON M.Id_Ejemplar = E.Id_Ejemplar
    INNER JOIN Libro L ON L.Id_Libro = E.Id_Libro
    INNER JOIN Estado S ON S.Id_Estado = M.Id_Estado
    WHERE M.Id_Usuario = @Id_Usuario
      AND M.Tipo = 'Préstamo'
      AND (M.Id_Estado = 4 OR M.Id_Estado = 5 OR M.Id_Estado = 7)
    ORDER BY M.Fecha DESC;
END

CREATE OR ALTER PROCEDURE RegistrarReservaPorAdmin
	@Id_Ejemplar INT,
    @Fecha DATETIME,
    @Fecha_Vencimiento DATETIME,
    @Id_Usuario INT
AS
BEGIN

	INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Estado, Id_Usuario, Id_Ejemplar)
    VALUES ('Préstamo', @Fecha, @Fecha_Vencimiento, 5, @Id_Usuario, @Id_Ejemplar);

	UPDATE Ejemplar SET Estado = 'Prestado' WHERE Id_Ejemplar = @Id_Ejemplar;

    SELECT 1 AS Resultado;
END;


-------------------OBTENER EJEMPLARES PARA ADMIN RESERVAR----------------------------------------------
CREATE OR ALTER PROCEDURE ObtenerEjemplaresDisponiblesAdmin
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
	WHERE E.Estado = 'Disponible';
END;

