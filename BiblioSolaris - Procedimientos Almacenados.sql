USE BiblioSolaris;
GO

/* ****************************************************************************************************
   *********************************** MODIFICACIONES DE LAS TABLAS ***********************************
   **************************************************************************************************** */

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

