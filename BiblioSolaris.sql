/* ****************************************************************************************************
   *********************************** CREACION DE LA BASE DE DATOS ***********************************
   **************************************************************************************************** */

   /*
   PARA EL PROFESOR: PARA UTILIZAR LAS FUNCIONALIDADES DE ADMINISTRADOR, SI TIENE QUE REGISTRARSE NORMAL
   Y CAMBIARSE EL ROL AQUI DESDE LA BD, YA QUE NO SE PUEDE CREAR UN USUARIO AQUI POR EL TEMA DE LA ENCRIPTACION
   DE LA CONTRASEÑA, SIMPLEMENTE PASAR EL ROL A 1 (ADMIN) EN VEZ DE 2 (CLIENTE Y DEFAULT AL REGISTRARSE)
   */

   /*
   PROFE CONSIDERE QUE ERAMOS 5 INTEGRANTES Y DOS DE ELLOS QUEDARON DEBIENDO Y SE ESPERARON HASTA EL ÚLTIMO 
   MOMENTO PARA EMPEZARLO, ESO COMPLICO EL RESULTADO DE ESTE AVANCE EN TEMAS DE RESERVAS, LA GESTIÓN COMO TAL DE 
   PRODUCTOS, Y EL DISEÑO, PERO TENGA CERTEZA QUE PARA LA ENTREGA FINAL SE TENDRÁ EL PRODUCTO COMPLETADO 
   Y UN MANEJO MUCHO MEJOR, A LO LARGO DE ESTAS SEMANAS NOS ESTAREMOS COMUNICANDO CON USTED PARA MOSTRARLE LAS MEJORAS
   EN LOS RESULTADOS 
   */



-- Eliminación y recreación de la base de datos
USE master;
IF DB_ID('BiblioSolaris') IS NOT NULL
BEGIN
    ALTER DATABASE BiblioSolaris SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BiblioSolaris;
END
GO

CREATE DATABASE BiblioSolaris;
GO

USE BiblioSolaris;
GO

/* ****************************************************************************************************
   ************************************** CREACION DE LAS TABLAS **************************************
   **************************************************************************************************** */


CREATE TABLE Estado( -- Activo, Inactivo, Pendiente, Completado, En Porceso
    Id_Estado INT IDENTITY(1,1) PRIMARY KEY,
    Estado VARCHAR(15) UNIQUE
);

CREATE TABLE Rol(
    Id_Rol INT IDENTITY(1,1) PRIMARY KEY,
    Tipo_Rol VARCHAR(25)
);

CREATE TABLE Categoria(
    Id_Categoria INT IDENTITY(1,1) PRIMARY KEY,
    Tipo VARCHAR(75) UNIQUE
);


CREATE TABLE Libro(
    Id_Libro INT IDENTITY(1,1) PRIMARY KEY,
    ISBN VARCHAR(20) UNIQUE,
    Titulo VARCHAR(200),
	Id_Categoria INT,
    Autor VARCHAR(150),
    Anio SMALLINT,
    Imagen_URL VARCHAR(600),
	Descripcion VARCHAR(150),
	FOREIGN KEY(Id_Categoria) REFERENCES Categoria(Id_Categoria)
	);

-- EJEMPLAR -- NUEVA
CREATE TABLE Ejemplar (
    Id_Ejemplar INT IDENTITY(1,1) PRIMARY KEY,
    CodigoEjemplar VARCHAR(20) UNIQUE NOT NULL,
    Id_Libro INT NOT NULL,
    Estado VARCHAR(50) NOT NULL DEFAULT('Disponible'),
    Ubicacion VARCHAR(100) NULL,
    Fecha_Registro DATETIME2 DEFAULT(GETDATE())
);
ALTER TABLE Ejemplar DROP COLUMN Cantidad;

ALTER TABLE Ejemplar
ADD CONSTRAINT FK_Ejemplar_Libro
FOREIGN KEY (Id_Libro)
REFERENCES Libro(Id_Libro)
ON DELETE CASCADE;
-------------------------------------------------------------------------------------------------------

CREATE TABLE Etiqueta( --- NUEVA
	Id_Etiqueta INT IDENTITY(1,1) PRIMARY KEY,
	Nombre VARCHAR(75) UNIQUE
);


CREATE TABLE Libro_Etiqueta(
	Id_Libro INT,
    Id_Etiqueta INT,
    PRIMARY KEY(Id_Libro, Id_Etiqueta),
    FOREIGN KEY (Id_Libro) REFERENCES Libro(Id_Libro),
    FOREIGN KEY (Id_Etiqueta) REFERENCES Etiqueta(Id_Etiqueta)
);

CREATE TABLE Usuario(
    Id_Usuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50),
    Apellidos VARCHAR(100),
	Identificacion VARCHAR(9) UNIQUE,
    Correo VARCHAR(75) UNIQUE,
    Contrasena VARCHAR(255),
    Telefono VARCHAR(15) UNIQUE,
    Id_Rol INT,
    Estado BIT, -- Estado Activo o Inactivo
    FOREIGN KEY (Id_Rol) REFERENCES Rol(Id_Rol),
);

CREATE TABLE Comentario(
    Id_Comentario INT IDENTITY(1,1) PRIMARY KEY,
    Creacion DATETIME2,
    Comentario VARCHAR(150),
    Rating INT,
    Id_Ejemplar INT,
    Id_Usuario INT,
    FOREIGN KEY (Id_Ejemplar) REFERENCES Ejemplar(Id_Ejemplar),
    FOREIGN KEY (Id_Usuario) REFERENCES Usuario(Id_Usuario)
);

CREATE TABLE Movimiento(
    Id_Movimiento INT IDENTITY(1,1) PRIMARY KEY,
    Tipo VARCHAR(30) DEFAULT('Préstamo'), -- 'PRESTAMO'
    Fecha DATETIME2,
    Fecha_Vencimiento DATETIME2 NULL,
    Id_Usuario INT,
    Id_Ejemplar INT,
	Id_Estado INT,
    FOREIGN KEY (Id_Usuario) REFERENCES Usuario(Id_Usuario),
    FOREIGN KEY (Id_Ejemplar) REFERENCES Ejemplar(Id_Ejemplar),
	FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado)
);

-- REVISAR
CREATE TABLE Sancion(
    Id_Sancion INT IDENTITY(1,1) PRIMARY KEY,
    Id_Movimiento INT,
	Estado BIT,
	Fecha_Finalizacion DATETIME DEFAULT(GETDATE()+14), --14 días de sanción sin poder reservar, una vez cumplida esa fecha, el estado se vuelve 0 y puede reservar de nuevo
    FOREIGN KEY(Id_Movimiento) REFERENCES Movimiento(Id_Movimiento)
);

CREATE TABLE Logs( -- Registro Diario
    Id_Log INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME2,
    Tipo_Accion VARCHAR(25),
    Descripcion_Accion VARCHAR(150),
    Modulo_Afectado VARCHAR(50),
    Id_Usuario INT,
    FOREIGN KEY (Id_Usuario) REFERENCES Usuario (Id_Usuario)
);



/* ****************************************************************************************************
   ********************************* MODIFICACIONES DE LA BD - TABLAS *********************************
   **************************************************************************************************** */
USE BiblioSolaris;
GO

-------------------------------------------------------------------------------------------------------

-- INSERT DE LOS ROLES 
INSERT INTO Rol(Tipo_Rol) VALUES
('Administrador'), ('Cliente');
-------------------------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------------------------

-- ALTER TABLE - VALIDAR QUE RATING ESTE ENTRE 1 Y 5
ALTER TABLE Comentario
ADD CONSTRAINT CK_Rating CHECK (Rating BETWEEN 1 AND 5);
-------------------------------------------------------------------------------------------------------

-- PREGUNTAS FRECUENTES - FREQUENTLY ASKED QUESTIONS

CREATE TABLE FAQ (
    Id_FAQ INT IDENTITY(1,1) PRIMARY KEY,
    Pregunta VARCHAR(255) NOT NULL,
    Respuesta VARCHAR(MAX) NOT NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);
-------------------------------------------------------------------------------------------------------
SELECT * FROM Estado;
-- INSERTS TABLA ESTADO
INSERT INTO Estado (Estado)
VALUES ('Activo'), ('Inactivo'), ('Disponible'), ('Pendiente'), ('Reservado'),('En Proceso'), ('Completado');
-------------------------------------------------------------------------------------------------------

/* ****************************************************************************************************
   ************************************ PROCEDIMIENTOS ALMACENADOS ************************************
   **************************************************************************************************** */

-- INICIAR SESION -------------------------------------------------------------------------------------
GO
CREATE PROCEDURE ObtenerUsuarioPorCorreo
	@Correo VARCHAR(75)
AS
BEGIN
	SELECT Id_Usuario,			
			Nombre,
			Apellidos,
			Identificacion,
			Correo,
			Contrasena,
			Telefono,
			Estado,
			U.Id_Rol,
			R.Tipo_Rol
			FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol
			WHERE Correo = @Correo AND Estado = 1
END


-- REGISTRARSE -------------------------------------------------------------------------------------
GO
CREATE PROCEDURE RegistroUsuario
	@Nombre VARCHAR(50),
	@Apellidos VARCHAR(100),
	@Identificacion VARCHAR(9),
	@Correo VARCHAR(75),
	@Contrasena VARCHAR(100),
	@Telefono VARCHAR(15)
AS
BEGIN
	IF EXISTS (SELECT 1 FROM Usuario WHERE Correo = @Correo OR Telefono = @Telefono)
	BEGIN
	SELECT 0 AS Resultado
	RETURN
	END
	
	DECLARE @Id_Rol INT = 2
	DECLARE @Estado BIT = 1

	INSERT INTO dbo.Usuario(Nombre, Apellidos, Identificacion, Correo, Contrasena, Telefono, Id_Rol, Estado)
	VALUES (@Nombre, @Apellidos, @Identificacion, @Correo, @Contrasena, @Telefono, @Id_Rol, @Estado)
	SELECT 1 AS Resultado
END


/* ****************************************************************************************************
   ********************************** NUEVAS TABLAS Y PROCEDIMIENTOS **********************************
   **************************************************************************************************** */

-- TABLA PARA REGISTRAR ERRORES DESDE EL API ----------------------------------------------------------
CREATE TABLE Error(
	ConsecutivoError INT PRIMARY KEY NOT NULL,
	Id_Usuario INT NOT NULL,
	Mensaje VARCHAR(MAX) NOT NULL,
	Origen VARCHAR(80) NOT NULL,
	FechaHora DATETIME NOT NULL
);

DROP TABLE Error;

CREATE TABLE Error(
	ConsecutivoError INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	Id_Usuario INT NOT NULL,
	Mensaje VARCHAR(MAX) NOT NULL,
	Origen VARCHAR(80) NOT NULL,
	FechaHora DATETIME NOT NULL
)
--------------------------------------------------------------------------------------------------------

-- SP PARA ERRORES -------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[RegistrarError]
	@Id_Usuario INT,
	@MensajeError VARCHAR(MAX),
	@OrigenError VARCHAR(80)
AS
BEGIN
	INSERT INTO Error (Id_Usuario, Mensaje, Origen, FechaHora)
	VALUES (@Id_Usuario, @MensajeError, @OrigenError, GETDATE())
END

-- SP PARA REGISTRAR USUARIOS (ADMIN) -------------------------------------------------------------------
GO
CREATE PROCEDURE RegistroUsuarioAdmin
	@Nombre VARCHAR(50),
	@Apellidos VARCHAR(100),
	@Identificacion VARCHAR(9),
	@Correo VARCHAR(75),
	@Contrasena VARCHAR(100),
	@Telefono VARCHAR(15),
	@Id_Rol INT
AS
BEGIN
	DECLARE @Estado BIT = 1
	IF NOT EXISTS(SELECT 1 FROM Usuario WHERE Correo = @Correo OR Identificacion = @Identificacion OR Telefono = @Telefono)
	BEGIN
		INSERT INTO dbo.Usuario(Nombre, Apellidos, Identificacion, Correo, Contrasena, Telefono, Id_Rol, Estado)
		VALUES (@Nombre, @Apellidos, @Identificacion, @Correo, @Contrasena, @Telefono, @Id_Rol, @Estado)

		SELECT @@IDENTITY 'Id_Usuario'
	END
	SELECT 0 'Id_Usuario'
END

-- SP PARA LISTAR USUARIOS -------------------------------------------------------------------------------------
GO
CREATE PROCEDURE ListarUsuarios
AS
BEGIN
	SELECT Id_Usuario, Nombre, Apellidos, Correo, Telefono, U.Id_Rol, Estado, Identificacion, R.Tipo_Rol
	FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol
END

-- SP PARA ACTUALIZAR USUARIO -------------------------------------------------------------------------------------
CREATE PROCEDURE ActualizarUsuarioAdmin --CAMBIO 2DO AVANCE
	@Id_Usuario INT,
	@Nombre VARCHAR(50),
	@Apellidos VARCHAR(100),
	@Identificacion VARCHAR(9),
	@Correo VARCHAR(75),
	@Telefono VARCHAR(15),
	@Id_Rol INT,
	@Estado BIT
AS
BEGIN
	UPDATE Usuario
	SET Identificacion= @Identificacion,
	Nombre = @Nombre,
	Apellidos = @Apellidos,
	Correo = @Correo,
	Telefono = @Telefono,
	Id_Rol = @Id_Rol,
	Estado = @Estado
	WHERE Id_Usuario = @Id_Usuario

	DECLARE @Resultado INT = @@ROWCOUNT;

	INSERT INTO Logs(Fecha, Tipo_Accion, Descripcion_Accion, Modulo_Afectado, Id_Usuario)
	VALUES (GETDATE(), 'Actualización', 'El administrador actualizó la información de un usuario', 'Usuario', @Id_Usuario );

	SELECT @Resultado AS Resultado;
END

-- SP PARA CAMBIAR CONTRASEÑA -------------------------------------------------------------------------------------
CREATE PROCEDURE CambiarContrasena
	@Id_Usuario INT,
	@Contrasena VARCHAR(100)
AS
BEGIN
	UPDATE Usuario
	SET Contrasena = @Contrasena
	WHERE Id_Usuario = @Id_Usuario
END

--------SP PARA FAQ-------------------
CREATE PROCEDURE ListarFAQActivas
AS
BEGIN
    SELECT Id_FAQ, Pregunta, Respuesta
    FROM FAQ
    WHERE Estado = 1
    ORDER BY Id_FAQ;
END;

CREATE PROCEDURE ListarFAQAdmin
AS
BEGIN
    SELECT Id_FAQ, Pregunta, Respuesta, Estado
    FROM FAQ
    ORDER BY Id_FAQ;
END;

CREATE PROCEDURE CrearFAQ
    @Pregunta VARCHAR(255),
    @Respuesta VARCHAR(MAX)
AS
BEGIN
    INSERT INTO FAQ (Pregunta, Respuesta)
    VALUES (@Pregunta, @Respuesta);
END;

CREATE PROCEDURE CambiarEstadoFAQ
    @Id_FAQ INT,
    @Estado BIT
AS
BEGIN
    UPDATE FAQ
    SET Estado = @Estado
    WHERE Id_FAQ = @Id_FAQ;
END;
--------------------------------------------------------------------------------------------------------
---------------------------------------- "OLVIDE MI CONTRASEÑA" ----------------------------------------
--------------------------------------------------------------------------------------------------------
-- SP PARA VALIDAR USUARIO ---------------------------------------------------------------------------------
CREATE PROCEDURE ValidarUsuario
	@Correo VARCHAR(75)
AS
BEGIN
	SELECT Id_Usuario,
			Identificacion,
			Nombre,
			Apellidos,
			Correo,
			Contrasena,
			Telefono,
			Estado,
			Id_Rol
			FROM Usuario
			WHERE Correo = @Correo AND Estado = 1
END

-- SP PARA CAMBIAR CONTRASEÑA -------------------------------------------------------------------------------------
CREATE PROCEDURE ActualizarContrasena
	@Id_Usuario INT,
	@Contrasena VARCHAR(255)
AS
BEGIN
	UPDATE Usuario
	SET Contrasena = @Contrasena
	WHERE Id_Usuario = @Id_Usuario
END

-- SP PARA EDITAR PERFIL -------------------------------------------------------------------------------------
CREATE PROCEDURE ActualizarPerfil
	@Id_Usuario INT,
	@Nombre VARCHAR(50),
	@Apellidos VARCHAR(100),
	@Identificacion VARCHAR(9),
	@Correo VARCHAR(75),
	@Telefono VARCHAR(15)
AS
BEGIN

	IF EXISTS(SELECT 1 FROM Usuario WHERE Id_Usuario <> @Id_Usuario AND(Identificacion = @Identificacion OR Correo = @Correo 
	OR Telefono = @Telefono))
	BEGIN
		SELECT 0 AS Resultado
		RETURN
	END
		UPDATE Usuario
		SET Identificacion= @Identificacion,
		Nombre = @Nombre,
		Apellidos = @Apellidos,
		Correo = @Correo,
		Telefono = @Telefono
		WHERE Id_Usuario = @Id_Usuario
	
		SELECT 1 AS Resultado
END

--------------------------------------------------------------------------------------------------------
-- SP PARA LISTAR ROLES -------------------------------------------------------------------------------------
CREATE PROCEDURE ListarRoles
AS
BEGIN
	SELECT Id_Rol, Tipo_Rol
	FROM Rol 
END

-- SP PARA ELIMINAR USUARIO ADMIN -------------------------------------------------------------------------------------
CREATE PROCEDURE EliminarUsuario --CAMBIO 2DO AVANCE
	@Id_Usuario INT
AS
BEGIN
	DELETE FROM Usuario WHERE Id_Usuario = @Id_Usuario
	INSERT INTO Logs(Fecha, Tipo_Accion, Descripcion_Accion, Modulo_Afectado,Id_Usuario)
	VALUES (GETDATE(),'Eliminar', 'Se eliminó un usuario', 'Usuario', @Id_Usuario);

END

-- SP PARA OBTENER USUARIO X ID -------------------------------------------------------------------------------------
CREATE PROCEDURE ObtenerUsuarioPorId
	@Id_Usuario INT
AS
BEGIN
	SELECT Id_Usuario, Nombre, Apellidos, Correo, Telefono, U.Id_Rol, R.Tipo_Rol, Estado, Identificacion 
	FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol WHERE Id_Usuario = @Id_Usuario
END

--------------------------------------------------------------------------------------------------------


-----------------------------------------LISTAR---------------------------------------------------------
CREATE PROCEDURE ListarLibros
AS
BEGIN
    SELECT 
        Id_Libro,
        ISBN,
		Descripcion,
		L.Id_Categoria,
		C.Tipo,
        Titulo,
        Autor,
        Anio,
        Imagen_URL
    FROM Libro L INNER JOIN Categoria C ON L.Id_Categoria = C.Id_Categoria;
END;

--------------------------------------------REGISTRAR----------------------------------------------
CREATE PROCEDURE RegistrarLibro
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

------------------------------------------ACTUALIZAR----------------------------------------------
CREATE PROCEDURE ActualizarLibro
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

-- ELIMINAR -------------------------------------------------------------------------------------
CREATE PROCEDURE EliminarLibro
	@Id_Usuario INT,
    @Id_Libro INT
AS
BEGIN
    DELETE FROM Libro
    WHERE Id_Libro = @Id_Libro;

	INSERT INTO Logs(Fecha, Tipo_Accion, Descripcion_Accion, Modulo_Afectado,Id_Usuario)
	VALUES (GETDATE(),'Eliminar', CONCAT('Se eliminó el libro ', @Id_Libro), 'Libro', @Id_Usuario);

END;

-- OBTENER LIBRO X ID -------------------------------------------------------------------------------------
CREATE PROCEDURE ObtenerLibroPorId
    @Id_Libro INT
AS
BEGIN
    SELECT 
        Id_Libro,
        ISBN,
        Titulo,
		Descripcion,
		L.Id_Categoria,
		C.Tipo,
        Autor,
        Anio,
        Imagen_URL
    FROM Libro L INNER JOIN Categoria C ON L.Id_Categoria = C.Id_Categoria
    WHERE Id_Libro = @Id_Libro;
END;

/*
******************************RESERVAS/PRESTAMOS NUEVOS MÉTODOS****************************************
*/

CREATE PROCEDURE ReservarLibro
(
    @Id_Libro INT,
    @Id_Usuario INT,
    @FechaInicio DATETIME2,
    @FechaFin DATETIME2
)
AS
BEGIN

    DECLARE @Id_Ejemplar INT;


    SELECT TOP 1 @Id_Ejemplar = Id_Ejemplar
    FROM Ejemplar
    WHERE Id_Libro = @Id_Libro
      AND Estado = 'Disponible'
    ORDER BY Id_Ejemplar;


    IF @Id_Ejemplar IS NULL
    BEGIN
        SELECT -1 AS Resultado;
        RETURN;
    END


    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Usuario, Id_Ejemplar, Id_Estado)
    VALUES ('Préstamo', @FechaInicio, @FechaFin, @Id_Usuario, @Id_Ejemplar, 6); 


    UPDATE Ejemplar
    SET Estado = 'Prestado'
    WHERE Id_Ejemplar = @Id_Ejemplar;
    SELECT 1 AS Resultado;
END;




/*********************************** LISTAR LIBROS NUEVOS-> NO SOBRE EJEMPLARES */

CREATE PROCEDURE ObtenerListaLibrosCliente
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
LEFT JOIN Ejemplar E 
    ON L.Id_Libro = E.Id_Libro 
    AND E.Estado = 'Disponible'
GROUP BY 
    L.Id_Libro, L.Titulo, L.Autor, L.Imagen_URL, L.Descripcion, L.ISBN, L.Anio
ORDER BY L.Titulo;
END

---------------------------------------- OBTENER RESERVAS ----------------------------------------
GO
CREATE OR ALTER PROCEDURE ObtenerReservasUsuario
    @Id_Usuario INT
AS
BEGIN
    SELECT 
        M.Id_Movimiento,
        M.Fecha,
        M.Fecha_Vencimiento,
        L.Id_Libro,
        L.Titulo,
        L.Imagen_URL,
        E.Estado
    FROM Movimiento M
    INNER JOIN Ejemplar EJ ON EJ.Id_Ejemplar = M.Id_Ejemplar
    INNER JOIN Libro L ON L.Id_Libro = EJ.Id_Libro
    INNER JOIN Estado E ON E.Id_Estado = M.Id_Estado
    WHERE M.Id_Usuario = @Id_Usuario
      AND M.Tipo = 'Reserva'
      AND M.Id_Estado <> 5
    ORDER BY M.Fecha DESC;
END;
GO


/******************************************* EJEMPLARES *******************************************/

CREATE PROCEDURE ObtenerEjemplares
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


CREATE PROCEDURE ObtenerEjemplarPorId
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


CREATE PROCEDURE RegistrarEjemplar
    @CodigoEjemplar VARCHAR(20),
    @Id_Libro INT,
	@Cantidad INT,
    @Estado VARCHAR(50),
    @Ubicacion VARCHAR(100)
AS
BEGIN
    INSERT INTO Ejemplar (CodigoEjemplar, Id_Libro, Estado, Ubicacion, Fecha_Registro)
    VALUES (@CodigoEjemplar, @Id_Libro, @Estado, @Ubicacion, GETDATE());
END

CREATE PROCEDURE ActualizarEjemplar
    @Id INT,
    @CodigoEjemplar VARCHAR(20),
	@Cantidad INT,
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

---------------------------------------- ELIMINAR EJEMPLAR ----------------------------------------
CREATE PROCEDURE EliminarEjemplar
    @Id INT
AS
BEGIN
    DELETE FROM Ejemplar WHERE Id_Ejemplar = @Id;
END
/* ****************************************************************************************************
   ******************************************** ESTADOS SP *********************************************
   **************************************************************************************************** */---------------------------------ESTADOS SP-----------------------------------------------------------
CREATE PROCEDURE ObtenerEstados
AS
BEGIN
	SELECT Id_Estado, Estado FROM Estado
END



/* ****************************************************************************************************
   ********************************** PROCEDIMIENTOS ALMACENADOS P4 ***********************************
   **************************** RESERVAS, NOTIFICACIONES, COMENTARIOS, FAQ ****************************
   **************************************************************************************************** */

-- RESERVAS DE LIBROS 
-- Crear reserva -------------------------------------------------------------------------------------
GO
CREATE OR ALTER PROCEDURE CrearReserva
    @Id_Usuario INT,
    @Id_Libro INT,
    @Fecha_Reserva DATETIME2,
    @Fecha_Expiracion DATETIME2,
    @Id_Estado INT
AS
BEGIN
    DECLARE @Id_Ejemplar INT;

    -- 1. Obtener ejemplar disponible
    SELECT TOP 1 @Id_Ejemplar = Id_Ejemplar
    FROM Ejemplar
    WHERE Id_Libro = @Id_Libro
      AND Estado = 'Disponible'
    ORDER BY Id_Ejemplar;

    -- 2. Validar disponibilidad
    IF @Id_Ejemplar IS NULL
    BEGIN
        SELECT -1 AS Resultado;
        RETURN;
    END

    -- 3. Crear reserva
    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Usuario, Id_Ejemplar, Id_Estado)
    VALUES ('RESERVA', @Fecha_Reserva, @Fecha_Expiracion, @Id_Usuario, @Id_Ejemplar, @Id_Estado);

    -- 4. Marcar ejemplar como reservado
    UPDATE Ejemplar
    SET Estado = 'Reservado'
    WHERE Id_Ejemplar = @Id_Ejemplar;

    SELECT 1 AS Resultado;
END;
GO

-- Cancelar reserva -------------------------------------------------------------------------------------
CREATE PROCEDURE CancelarReserva
    @Id_Movimiento INT,
    @Id_Estado INT -- Cancelada
AS
BEGIN
    UPDATE Movimiento
    SET Id_Estado = @Id_Estado
    WHERE Id_Movimiento = @Id_Movimiento AND Tipo = 'RESERVA'
END

-- Expirar reserva -------------------------------------------------------------------------------------
CREATE PROCEDURE ExpirarReserva
AS
BEGIN
    UPDATE Movimiento
    SET Id_Estado = (SELECT Id_Estado FROM Estado WHERE Estado = 'Expirada')
    WHERE Tipo = 'RESERVA' AND Fecha_Vencimiento < GETDATE() AND Id_Estado <> (SELECT Id_Estado FROM Estado WHERE Estado = 'Expirada')
END


-- NOTIFICACIONES POR CORREO
-- Registrar envio de correo -------------------------------------------------------------------------------------
CREATE PROCEDURE RegistrarCorreoReserva
    @Id_Usuario INT,
    @Descripcion VARCHAR(150)
AS
BEGIN
    INSERT INTO Logs (Fecha, Tipo_Accion, Descripcion_Accion, Modulo_Afectado, Id_Usuario)
    VALUES (GETDATE(), 'Correo', @Descripcion, 'Reservas', @Id_Usuario)
END


-- COMENTARIOS Y CALIFICACIONES
-- Agregar comentario -------------------------------------------------------------------------------------
GO
CREATE OR ALTER PROCEDURE AgregarComentario
    @Id_Usuario INT,
    @Id_Libro INT,
    @Comentario VARCHAR(150),
    @Rating INT
AS
BEGIN
    DECLARE @Id_Ejemplar INT;

    -- 1. Obtener un ejemplar del libro (el más reciente usado o cualquiera)
    SELECT TOP 1 @Id_Ejemplar = E.Id_Ejemplar
    FROM Ejemplar E
    INNER JOIN Movimiento M ON M.Id_Ejemplar = E.Id_Ejemplar
    WHERE E.Id_Libro = @Id_Libro
      AND M.Id_Usuario = @Id_Usuario
    ORDER BY M.Fecha DESC;

    IF @Id_Ejemplar IS NULL
    BEGIN
        SELECT -1 AS Resultado;
        RETURN;
    END

    -- 2. Insertar comentario
    INSERT INTO Comentario (Creacion, Comentario, Rating, Id_Ejemplar, Id_Usuario)
    VALUES (GETDATE(), @Comentario, @Rating, @Id_Ejemplar, @Id_Usuario);

    SELECT 1 AS Resultado;
END;
GO
-- Listar comentarios por libro -------------------------------------------------------------------------------------
GO
CREATE OR ALTER PROCEDURE ListarComentariosPorLibro
    @Id_Libro INT
AS
BEGIN
    SELECT 
        C.Id_Comentario,
        C.Comentario,
        C.Rating,
        C.Creacion,
        U.Nombre,
        U.Apellidos
    FROM Comentario C
    INNER JOIN Usuario U ON C.Id_Usuario = U.Id_Usuario
    INNER JOIN Ejemplar E ON C.Id_Ejemplar = E.Id_Ejemplar
    WHERE E.Id_Libro = @Id_Libro
    ORDER BY C.Creacion DESC;
END;
GO

-- PREGUNTAS FRECUENTES
-- Crear pregunta -------------------------------------------------------------------------------------
CREATE PROCEDURE CrearFAQ
    @Pregunta VARCHAR(255),
    @Respuesta TEXT,
    @Estado BIT
AS
BEGIN
    INSERT INTO FAQ (Pregunta, Respuesta, Estado)
    VALUES (@Pregunta, @Respuesta, @Estado)
END

-- Listar pregunta -------------------------------------------------------------------------------------
CREATE PROCEDURE ListarFAQsActivas
AS
BEGIN
    SELECT Id_FAQ, Pregunta, Respuesta
    FROM FAQ
    WHERE Estado = 1
END

--------------------------------------------------------------------------------------------------------

/* ****************************************************************************************************
   ********************************** EJECUCIONES PARA REVISAR DATOS **********************************
   **************************************************************************************************** */

-- SELECT 
SELECT TOP 1 * FROM Libro;
--------------------------------------------------------------------------------------------------------


/*
	************************************REPORTES Y OBTENCION DE DATOS RELEVANTES************************
*/

CREATE PROCEDURE ObtenerReservasActivas
AS
BEGIN
	SELECT Id_Movimiento, Tipo, Fecha, Fecha_Vencimiento, M.Id_Estado, CONCAT(U.Nombre, ' ',U.Apellidos) AS Nombre, L.Titulo
	FROM Movimiento M INNER JOIN Usuario U ON M.Id_Usuario = U.Id_Usuario 
	INNER JOIN Libro L ON M.Id_Ejemplar = L.Id_Libro
	AND M.Id_Estado = 1
END;
--EXEC ObtenerReservasActivas;

CREATE PROCEDURE ObtenerLibrosConMasCantidadMovimientos
AS
BEGIN
    SELECT TOP 5
        L.Id_Libro,
        L.Titulo,
        COUNT(DISTINCT M.Id_Movimiento) AS CantidadMovimientos
    FROM Libro L
    INNER JOIN Ejemplar E ON L.Id_Libro = E.Id_Libro
    INNER JOIN Movimiento M ON E.Id_Ejemplar = M.Id_Ejemplar
    
    GROUP BY L.Id_Libro, L.Titulo
    ORDER BY CantidadMovimientos DESC;
END;

EXEC ObtenerLibrosConMasCantidadMovimientos;

-------------------------------USUARIOS CON MÁS SANCIONES--------------------------------------------------

---------------------PENDIENTE--------------------------


-------------------------------TEMAS DE BITACORAS/AUDITLOGS------------------------------------------------

-----------------------------------------------------------------------------------------------------------

------------------------------SP Categorias--------------------------------------------------------------

CREATE PROCEDURE RegistrarCategoria
@Tipo VARCHAR(75)
AS
BEGIN
	INSERT INTO Categoria(Tipo) VALUES
	(@Tipo)
END;

CREATE PROCEDURE ObtenerCategorias
AS
BEGIN
	SELECT Id_Categoria, Tipo FROM Categoria
END;

CREATE PROCEDURE EditarCategoria 
@Id_Categoria INT,
@Tipo VARCHAR(75)
AS
BEGIN
	UPDATE Categoria SET Tipo = @Tipo WHERE Id_Categoria = @Id_Categoria
END;

------------------------------------------
CREATE PROCEDURE ReservarLibro
(
    @Id_Libro INT,
    @Id_Usuario INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME
)
AS
BEGIN
    DECLARE @Id_Ejemplar INT;

    SELECT TOP 1 @Id_Ejemplar = Id_Ejemplar
    FROM Ejemplar
    WHERE Id_Libro = @Id_Libro AND Estado = 'Disponible';

    IF @Id_Ejemplar IS NULL
    BEGIN
        SELECT -1 AS Resultado;
        RETURN;
    END

    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Usuario, Id_Ejemplar, Id_Estado)
    VALUES ('RESERVA', @FechaInicio, @FechaFin, @Id_Usuario, @Id_Ejemplar, 2);

    UPDATE Ejemplar SET Estado = 'Reservado' WHERE Id_Ejemplar = @Id_Ejemplar;

    SELECT 1 AS Resultado;
END


ALTER PROCEDURE AgregarComentario
    @Id_Usuario INT,
    @Id_Ejemplar INT,
    @Comentario VARCHAR(150),
    @Rating INT
AS
BEGIN
    INSERT INTO Comentario (Creacion, Comentario, Rating, Id_Ejemplar, Id_Usuario)
    VALUES (GETDATE(), @Comentario, @Rating, @Id_Ejemplar, @Id_Usuario)
END

ALTER PROCEDURE ListarComentariosPorLibro
    @Id_Libro INT
AS
BEGIN
    SELECT 
        C.Id_Comentario,
        C.Comentario,
        C.Rating,
        C.Creacion,
        CONCAT(U.Nombre, ' ', U.Apellidos) AS NombreUsuario
    FROM Comentario C
    INNER JOIN Usuario U ON C.Id_Usuario = U.Id_Usuario
    INNER JOIN Ejemplar E ON C.Id_Ejemplar = E.Id_Ejemplar
    WHERE E.Id_Libro = @Id_Libro
END


SELECT * FROM FAQ;
EXEC CrearFAQ '¿Ejemplo?', 'Respuesta de prueba';
EXEC ListarFAQActivas;

EXEC CrearFAQ 
    @Pregunta = '¿Horario de atención?',
    @Respuesta = 'De lunes a viernes',
    @Estado = 1;

	SELECT * FROM Rol;

	SELECT * FROM Usuario;

	UPDATE Usuario
SET Id_Rol = 1
WHERE Id_Usuario = 1;


SELECT name
FROM sys.procedures
WHERE name LIKE '%Reserva%'