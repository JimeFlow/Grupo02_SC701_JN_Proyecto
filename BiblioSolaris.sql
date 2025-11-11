/* ****************************************************************************************************
   *********************************** CREACION DE LA BASE DE DATOS ***********************************
   **************************************************************************************************** */

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

---------------------------ALTER TABLE AGREGAR COLUMNA IDENTIFICACION----------------------------
ALTER TABLE Usuario ADD Identificacion VARCHAR(9);
ALTER TABLE Usuario ADD CONSTRAINT IDENTIFICACION_UNIQUE UNIQUE(Identificacion);
--------------------------------------------------------------------------------------------------

-----------------------------INSERT DE LOS ROLES--------------------------------------------------

INSERT INTO Rol(Tipo_Rol) VALUES
('Administrador'), ('Cliente');
--------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------

----------------------------HACER MAS GRANDE LAS CONTRASENAS--------------------------------------
ALTER TABLE Usuario ALTER COLUMN Contrasena VARCHAR(255);
--------------------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------------------
----------------------------PROCEDIMIENTOS ALMACENADOS-------------------------------------------

--INICIAR SESION
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



-- REGISTRARSE
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


----------------------------------TABLA PARA REGISTRAR ERRORES DESDE EL API---------------------------------------

CREATE TABLE Error(
	ConsecutivoError INT PRIMARY KEY NOT NULL,
	Id_Usuario INT NOT NULL,
	Mensaje VARCHAR(MAX) NOT NULL,
	Origen VARCHAR(80) NOT NULL,
	FechaHora DATETIME NOT NULL
)

-------------SP PARA ERRORES---------------------------------------------------------------------
CREATE PROCEDURE [dbo].[RegistrarError]
	@Id_Usuario INT,
	@MensajeError VARCHAR(MAX),
	@OrigenError VARCHAR(80)
	
AS
BEGIN

	INSERT INTO Error (Id_Usuario, Mensaje, Origen, FechaHora)
	VALUES (@Id_Usuario, @MensajeError, @OrigenError, GETDATE())
END

--------------------------SP PARA REGISTRAR USUARIOS (ADMIN) ---------------------------------------

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

----------------------------------LISTAR USUARIOS -------------------------------------------------------------------

CREATE PROCEDURE ListarUsuarios
AS
BEGIN
	SELECT Id_Usuario, Nombre, Apellidos, Correo, Telefono, U.Id_Rol, Estado, Identificacion, R.Tipo_Rol
	FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol
END

-----------------------------------SP PARA ACTUALIZAR USUARIO -------------------------------------------------------

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


----------------------------SP PARA CAMBIAR CONTRASEÑA----------------------------------------------------------

CREATE PROCEDURE CambiarContrasena
	@Id_Usuario INT,
	@Contrasena VARCHAR(100)
AS
BEGIN
	UPDATE Usuario
	SET Contrasena = @Contrasena
	WHERE Id_Usuario = @Id_Usuario
END

-----------------------------------------------------------------------------------------------------------------

---------------------------SP PARA "OLVIDE MI CONTRASEÑA"--------------------------------------------------------
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

------------------------SP PARA CAMBIAR CONTRASEÑA-----------------------------------------------------------------

CREATE PROCEDURE ActualizarContrasena
	@Id_Usuario INT,
	@Contrasena VARCHAR(255)
AS
BEGIN
	UPDATE Usuario
	SET Contrasena = @Contrasena
	WHERE Id_Usuario = @Id_Usuario
END

-----------------------EDITAR PERFIL PROPIO--------------------------------------------------------------------------

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

---------------------------------------------------------------------------------------------------------------------

------------------------------------------LISTAR ROLES---------------------------------------------------------------

CREATE PROCEDURE ListarRoles
AS
BEGIN
	SELECT Id_Rol, Tipo_Rol
	FROM Rol 
END

------------------------------------------ELIMINAR USUARIO ADMIN-----------------------------------------------------

CREATE PROCEDURE EliminarUsuario
	@Id_Usuario INT
AS
BEGIN
	DELETE FROM Usuario WHERE Id_Usuario = @Id_Usuario
END
------------------------------------------OBTENER USUARIO X ID ------------------------------------------------------

CREATE PROCEDURE ObtenerUsuarioPorId
	@Id_Usuario INT
AS
BEGIN
	SELECT Id_Usuario, Nombre, Apellidos, Correo, Telefono, U.Id_Rol, R.Tipo_Rol, Estado, Identificacion 
	FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol WHERE Id_Usuario = @Id_Usuario
END

---------------------------------------------------------------------------------------------------------------------

