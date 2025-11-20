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

CREATE TABLE Autor(
    Id_Autor INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100),
    Apellidos VARCHAR(100),
    Nacionalidad VARCHAR(50),
    Fecha_Nacimiento DATE,
    Id_Estado INT,
    FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado)
);

-- Campo para Imagen, ya sea que se guarde en la BD o como URL en el proyecto
-- Se puede normalizar creando una TABLA DE AUTOR y una FK aqui
CREATE TABLE Libro(
    Id_Libro INT IDENTITY(1,1) PRIMARY KEY,
    ISBN VARCHAR(20) UNIQUE,
    Estado_Libro VARCHAR(100),
    Titulo VARCHAR(200),
    Autor VARCHAR(150),
    Anio SMALLINT,
    Imagen_URL VARCHAR(600),     
    Id_Estado INT,
    FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado)
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

ALTER TABLE Ejemplar
ADD CONSTRAINT FK_Ejemplar_Libro
FOREIGN KEY (Id_Libro)
REFERENCES Libro(Id_Libro)
ON DELETE CASCADE;
-------------------------------------------------------------------------------------------------------

CREATE TABLE Libro_Etiquetas( --- NUEVA
    Id_Libro INT,
    Id_Categoria INT,
    PRIMARY KEY(Id_Libro, Id_Categoria),
    FOREIGN KEY (Id_Libro) REFERENCES Libro(Id_Libro),
    FOREIGN KEY (Id_Categoria) REFERENCES Categoria(Id_Categoria)
);

CREATE TABLE Alerta( -- NUEVA - Administrador pueda registrar inconvenientes o situaciones
    Id_Alerta INT IDENTITY(1,1) PRIMARY KEY,
    Comentario VARCHAR(255),
    Fecha DATETIME2,
    Id_Libro INT,
    FOREIGN KEY (Id_Libro) REFERENCES Libro(Id_Libro)
);

CREATE TABLE Inventario(
    Id_Inventario INT IDENTITY(1,1) PRIMARY KEY,
    Id_Libro INT,
    Stock INT,
    Id_Estado INT,
    FOREIGN KEY(Id_Libro) REFERENCES Libro(Id_Libro),
    FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado)
);

CREATE TABLE Entrega(
    Id_Entrega INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME2,
    Id_Libro INT,
    Cantidad INT,
    Id_Estado INT,
    FOREIGN KEY(Id_Libro) REFERENCES Libro(Id_Libro),
    FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado)
);

CREATE TABLE Ingreso(
    Id_Ingreso INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME2,
    Id_Libro INT,
    Cantidad INT,
    Id_Estado INT,
    FOREIGN KEY (Id_Libro) REFERENCES Libro(Id_Libro),
    FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado)
);

CREATE TABLE Usuario(
    Id_Usuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50),
    Apellidos VARCHAR(100),
    Correo VARCHAR(75) UNIQUE,
    Contrasena VARCHAR(100),
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
    Id_Libro INT,
    Id_Usuario INT,
    FOREIGN KEY (Id_Libro) REFERENCES Libro(Id_Libro),
    FOREIGN KEY (Id_Usuario) REFERENCES Usuario(Id_Usuario)
);

CREATE TABLE Movimiento(
    Id_Movimiento INT IDENTITY(1,1) PRIMARY KEY,
    Tipo VARCHAR(10), -- 'RESERVA' o 'PRESTAMO'
    Fecha DATETIME2,
    Fecha_Vencimiento DATETIME2 NULL,
    Estado INT,
    Id_Usuario INT,
    Id_Libro INT,
    FOREIGN KEY (Id_Usuario) REFERENCES Usuario(Id_Usuario),
    FOREIGN KEY (Id_Libro) REFERENCES Libro(Id_Libro)
);

-- REVISAR
CREATE TABLE Sancion(
    Id_Sancion INT IDENTITY(1,1) PRIMARY KEY,
    Id_Movimiento INT,
    --Monto DECIMAL(10,2),
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

-- ALTER TABLE AGREGAR COLUMNA IDENTIFICACION
ALTER TABLE Usuario ADD Identificacion VARCHAR(9);
ALTER TABLE Usuario ADD CONSTRAINT IDENTIFICACION_UNIQUE UNIQUE(Identificacion);
-------------------------------------------------------------------------------------------------------

-- INSERT DE LOS ROLES 
INSERT INTO Rol(Tipo_Rol) VALUES
('Administrador'), ('Cliente');
-------------------------------------------------------------------------------------------------------

-- ALTER TABLE HACER MAS GRANDE LAS CONTRASENAS
ALTER TABLE Usuario ALTER COLUMN Contrasena VARCHAR(255);
-------------------------------------------------------------------------------------------------------

-- ALTER TABLE - ESTADO A FK ID_ESTADO
ALTER TABLE Movimiento ADD Id_Estado INT;
ALTER TABLE Movimiento ADD FOREIGN KEY (Id_Estado) REFERENCES Estado(Id_Estado);
-------------------------------------------------------------------------------------------------------

-- ALTER TABLE - VALIDAR QUE RATING ESTE ENTRE 1 Y 5
ALTER TABLE Comentario
ADD CONSTRAINT CK_Rating CHECK (Rating BETWEEN 1 AND 5);
-------------------------------------------------------------------------------------------------------

-- PREGUNTAS FRECUENTES - FREQUENTLY ASKED QUESTIONS
CREATE TABLE FAQ (
    Id_FAQ INT IDENTITY(1,1) PRIMARY KEY,
    Pregunta VARCHAR(255),
    Respuesta TEXT,
    Estado BIT -- 1 = Activa / 0 = Oculta
);
-------------------------------------------------------------------------------------------------------

-- INSERTS TABLA ESTADO
INSERT INTO Estado (Estado)
VALUES ('Activo'), ('Inactivo'), ('Disponible'), ('Reservado'), ('Pendiente'),('En Proceso'), ('Completado');
-------------------------------------------------------------------------------------------------------

/* ****************************************************************************************************
   ************************************ PROCEDIMIENTOS ALMACENADOS ************************************
   **************************************************************************************************** */

-- INICIAR SESION -------------------------------------------------------------------------------------
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
CREATE PROCEDURE ListarUsuarios
AS
BEGIN
	SELECT Id_Usuario, Nombre, Apellidos, Correo, Telefono, U.Id_Rol, Estado, Identificacion, R.Tipo_Rol
	FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol
END

-- SP PARA ACTUALIZAR USUARIO -------------------------------------------------------------------------------------
CREATE PROCEDURE ActualizarUsuarioAdmin
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
CREATE PROCEDURE EliminarUsuario
	@Id_Usuario INT
AS
BEGIN
	DELETE FROM Usuario WHERE Id_Usuario = @Id_Usuario
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


/* ****************************************************************************************************
   ******************************************** LIRBOS SP *********************************************
   **************************************************************************************************** */

-- LISTAR -------------------------------------------------------------------------------------
CREATE PROCEDURE ListarLibros
AS
BEGIN
    SELECT 
        Id_Libro,
        ISBN,
        Estado_Libro,
        Titulo,
        Autor,
        Anio,
        Imagen_URL,
        Id_Estado
    FROM Libro;
END;

-- REGISTRAR -------------------------------------------------------------------------------------
CREATE PROCEDURE RegistrarLibro
    @ISBN VARCHAR(20),
    @Estado_Libro VARCHAR(100),
    @Titulo VARCHAR(200),
    @Autor VARCHAR(150),
    @Anio INT,
    @Imagen_URL VARCHAR(600),
    @Id_Estado INT
AS
BEGIN
    INSERT INTO Libro(ISBN, Estado_Libro, Titulo, Autor, Anio, Imagen_URL, Id_Estado)
    VALUES (@ISBN, @Estado_Libro, @Titulo, @Autor, @Anio, @Imagen_URL, @Id_Estado);
END;

-- ACTUALIZAR -------------------------------------------------------------------------------------
CREATE PROCEDURE ActualizarLibro
    @Id_Libro INT,
    @ISBN VARCHAR(20),
    @Estado_Libro VARCHAR(100),
    @Titulo VARCHAR(200),
    @Autor VARCHAR(150),
    @Anio INT,
    @Imagen_URL VARCHAR(600),
    @Id_Estado INT
AS
BEGIN
    UPDATE Libro
    SET 
        ISBN = @ISBN,
        Estado_Libro = @Estado_Libro,
        Titulo = @Titulo,
        Autor = @Autor,
        Anio = @Anio,
        Imagen_URL = @Imagen_URL,
        Id_Estado = @Id_Estado
    WHERE Id_Libro = @Id_Libro;
END;

-- ELIMINAR -------------------------------------------------------------------------------------
CREATE PROCEDURE EliminarLibro
    @Id_Libro INT
AS
BEGIN
    DELETE FROM Libro
    WHERE Id_Libro = @Id_Libro;
END;

-- OBTENER LIBRO X ID -------------------------------------------------------------------------------------
CREATE PROCEDURE ObtenerLibroPorId
    @Id_Libro INT
AS
BEGIN
    SELECT 
        Id_Libro,
        ISBN,
        Estado_Libro,
        Titulo,
        Autor,
        Anio,
        Imagen_URL,
        Id_Estado
    FROM Libro
    WHERE Id_Libro = @Id_Libro;
END;

-- RESERVAR LIRBO -------------------------------------------------------------------------------------
CREATE PROCEDURE ReservarLibro
    @Id_Libro INT,
    @Tipo VARCHAR(10),
    @Fecha DATETIME,
    @Fecha_Vencimiento DATETIME,
    @Estado INT,
    @Id_Usuario INT
AS
BEGIN
    DECLARE @EstadoActual VARCHAR(100);

    SELECT @EstadoActual = Estado_Libro
    FROM Libro
    WHERE Id_Libro = @Id_Libro;

    IF (@EstadoActual <> 'Disponible')
    BEGIN
        SELECT -1 AS Resultado;
        RETURN;
    END

    UPDATE Libro
    SET Estado_Libro = 'Reservado',
        Id_Estado = 2
    WHERE Id_Libro = @Id_Libro;

    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Estado, Id_Usuario, Id_Libro)
    VALUES (@Tipo, @Fecha, @Fecha_Vencimiento, @Estado, @Id_Usuario, @Id_Libro);

    SELECT 1 AS Resultado;
END;


/******************************************* RESERVAS *******************************************

CREATE PROCEDURE CrearReserva
    @Id_Usuario INT,
    @Id_Libro INT,
    @Fecha_Reserva DATETIME,
    @Fecha_Vencimiento DATETIME,
    @Id_Estado INT
AS
BEGIN
    INSERT INTO Movimiento (Id_Usuario, Id_Libro, Fecha, Fecha_Vencimiento, Tipo, Id_Estado)
    VALUES (@Id_Usuario, @Id_Libro, @Fecha_Reserva, @Fecha_Vencimiento, 'Reserva', @Id_Estado);

    -- Poner libro como "No disponible"
    UPDATE Libro SET Id_Estado = 2 WHERE Id_Libro = @Id_Libro;
END

*/

---------------------------------------- OBTENER RESERVAS ----------------------------------------
CREATE PROCEDURE ObtenerReservasUsuario
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
    INNER JOIN Libro L ON L.Id_Libro = M.Id_Libro
    INNER JOIN Estado E ON E.Id_Estado = M.Id_Estado
    WHERE M.Id_Usuario = @Id_Usuario
      AND M.Tipo = 'Reserva'
      AND M.Id_Estado <> 5
    ORDER BY M.Fecha DESC;
END

---------------------------------------- CANCELAR RESERVA ----------------------------------------
CREATE PROCEDURE CancelarReserva
    @Id_Movimiento INT
AS
BEGIN

    UPDATE Movimiento
    SET Id_Estado = 5
    WHERE Id_Movimiento = @Id_Movimiento;

    UPDATE Libro
    SET Id_Estado = 1
    WHERE Id_Libro = (SELECT Id_Libro FROM Movimiento WHERE Id_Movimiento = @Id_Movimiento);
END


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

---------------------------------------- OBTENER EJEMPLAR POR ID ----------------------------------------
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

---------------------------------------- REGISTRAR EJEMPLAR ----------------------------------------
CREATE PROCEDURE RegistrarEjemplar
    @CodigoEjemplar VARCHAR(20),
    @Id_Libro INT,
    @Estado VARCHAR(50),
    @Ubicacion VARCHAR(100)
AS
BEGIN
    INSERT INTO Ejemplar (CodigoEjemplar, Id_Libro, Estado, Ubicacion, Fecha_Registro)
    VALUES (@CodigoEjemplar, @Id_Libro, @Estado, @Ubicacion, GETDATE());
END

---------------------------------------- ACTUALIZAR EJEMPLAR ----------------------------------------
CREATE PROCEDURE ActualizarEjemplar
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
CREATE PROCEDURE CrearReserva
    @Id_Usuario INT,
    @Id_Libro INT,
    @Fecha_Reserva DATETIME2,
    @Fecha_Expiracion DATETIME2,
    @Id_Estado INT -- Pendiente
AS
BEGIN
    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Usuario, Id_Libro, Id_Estado)
    VALUES ('RESERVA', @Fecha_Reserva, @Fecha_Expiracion, @Id_Usuario, @Id_Libro, @Id_Estado)
END

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
CREATE PROCEDURE AgregarComentario
    @Id_Usuario INT,
    @Id_Libro INT,
    @Comentario VARCHAR(150),
    @Rating INT
AS
BEGIN
    INSERT INTO Comentario (Creacion, Comentario, Rating, Id_Libro, Id_Usuario)
    VALUES (GETDATE(), @Comentario, @Rating, @Id_Libro, @Id_Usuario)
END

-- Listar comentarios por libro -------------------------------------------------------------------------------------
CREATE PROCEDURE ListarComentariosPorLibro
    @Id_Libro INT
AS
BEGIN
    SELECT C.Id_Comentario, C.Comentario, C.Rating, C.Creacion, U.Nombre, U.Apellidos
    FROM Comentario C
    INNER JOIN Usuario U ON C.Id_Usuario = U.Id_Usuario
    WHERE C.Id_Libro = @Id_Libro
END


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
