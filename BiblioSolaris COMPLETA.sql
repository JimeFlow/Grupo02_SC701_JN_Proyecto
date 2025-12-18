
/* ===================== 1. CREACIÓN DE BASE ===================== */
USE master;
GO
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

/* ===================== 2. TABLAS ===================== */

CREATE TABLE Estado(
    Id_Estado INT IDENTITY(1,1) PRIMARY KEY,
    Estado VARCHAR(20) UNIQUE
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
    Descripcion VARCHAR(150),
    Autor VARCHAR(150),
    Anio INT,
    Imagen_URL VARCHAR(600),
    Id_Categoria INT,
    FOREIGN KEY(Id_Categoria) REFERENCES Categoria(Id_Categoria)
);

CREATE TABLE Ejemplar(
    Id_Ejemplar INT IDENTITY(1,1) PRIMARY KEY,
    CodigoEjemplar VARCHAR(20) UNIQUE NOT NULL,
    Id_Libro INT NOT NULL,
    Estado VARCHAR(50) DEFAULT 'Disponible',
    Ubicacion VARCHAR(100),
    Fecha_Registro DATETIME DEFAULT GETDATE(),
    FOREIGN KEY(Id_Libro) REFERENCES Libro(Id_Libro)
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
    Estado BIT,
    FOREIGN KEY(Id_Rol) REFERENCES Rol(Id_Rol)
);

CREATE TABLE Movimiento(
    Id_Movimiento INT IDENTITY(1,1) PRIMARY KEY,
    Tipo VARCHAR(30),
    Fecha DATETIME,
    Fecha_Vencimiento DATETIME,
    Id_Usuario INT,
    Id_Ejemplar INT,
    Id_Estado INT,
    FOREIGN KEY(Id_Usuario) REFERENCES Usuario(Id_Usuario),
    FOREIGN KEY(Id_Ejemplar) REFERENCES Ejemplar(Id_Ejemplar),
    FOREIGN KEY(Id_Estado) REFERENCES Estado(Id_Estado)
);

CREATE TABLE Comentario(
    Id_Comentario INT IDENTITY(1,1) PRIMARY KEY,
    Creacion DATETIME,
    Comentario VARCHAR(150),
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Id_Ejemplar INT,
    Id_Usuario INT,
    FOREIGN KEY(Id_Ejemplar) REFERENCES Ejemplar(Id_Ejemplar),
    FOREIGN KEY(Id_Usuario) REFERENCES Usuario(Id_Usuario)
);

CREATE TABLE Sancion(
    Id_Sancion INT IDENTITY(1,1) PRIMARY KEY,
    Id_Usuario INT,
    Estado BIT DEFAULT 1,
    Fecha_Inicio DATETIME DEFAULT GETDATE(),
    Fecha_Finalizacion DATETIME DEFAULT DATEADD(DAY,14,GETDATE()),
    FOREIGN KEY(Id_Usuario) REFERENCES Usuario(Id_Usuario)
);

CREATE TABLE FAQ(
    Id_FAQ INT IDENTITY(1,1) PRIMARY KEY,
    Pregunta VARCHAR(255),
    Respuesta VARCHAR(MAX),
    Estado BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE()
);

CREATE TABLE Contacto(
    Id_Contacto INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100),
    Correo VARCHAR(150),
    Asunto VARCHAR(150),
    Mensaje VARCHAR(MAX),
    Respuesta VARCHAR(MAX),
    FechaEnvio DATETIME DEFAULT GETDATE(),
    FechaRespuesta DATETIME,
    Estado BIT DEFAULT 1
);

CREATE TABLE Logs(
    Id_Log INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME,
    Tipo_Accion VARCHAR(25),
    Descripcion_Accion VARCHAR(150),
    Modulo_Afectado VARCHAR(50),
    Id_Usuario INT
);

CREATE TABLE Error(
    ConsecutivoError INT IDENTITY(1,1) PRIMARY KEY,
    Id_Usuario INT,
    Mensaje VARCHAR(MAX),
    Origen VARCHAR(80),
    FechaHora DATETIME
);

/* ===================== 3. DATOS BASE ===================== */

INSERT INTO Rol(Tipo_Rol) VALUES ('Administrador'), ('Cliente');

INSERT INTO Estado(Estado)
VALUES ('Activo'),('Cancelado'),('Disponible'),('Pendiente'),
       ('Reservado'),('Prestado'),('Completado');

/* ===================== 4. PROCEDIMIENTOS ===================== */
/* ===== USUARIOS ===== */

CREATE OR ALTER PROCEDURE ObtenerUsuarioPorCorreo
@Correo VARCHAR(75)
AS
BEGIN
    SELECT U.*, R.Tipo_Rol
    FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol
    WHERE Correo = @Correo AND Estado = 1;
END
GO

CREATE OR ALTER PROCEDURE RegistroUsuario
@Nombre VARCHAR(50),
@Apellidos VARCHAR(100),
@Identificacion VARCHAR(9),
@Correo VARCHAR(75),
@Contrasena VARCHAR(255),
@Telefono VARCHAR(15)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Usuario WHERE Correo=@Correo OR Telefono=@Telefono)
    BEGIN
        SELECT 0 AS Resultado; RETURN;
    END
    INSERT INTO Usuario
    VALUES(@Nombre,@Apellidos,@Identificacion,@Correo,@Contrasena,@Telefono,2,1);
    SELECT 1 AS Resultado;
END
GO

/* ===== LIBROS ===== */

CREATE OR ALTER PROCEDURE ListarLibros
AS
BEGIN
    SELECT L.*, C.Tipo
    FROM Libro L INNER JOIN Categoria C ON L.Id_Categoria = C.Id_Categoria;
END
GO

CREATE OR ALTER PROCEDURE RegistrarLibro
@ISBN VARCHAR(20),
@Descripcion VARCHAR(150),
@Titulo VARCHAR(200),
@Autor VARCHAR(150),
@Anio INT,
@Imagen_URL VARCHAR(600),
@Id_Categoria INT
AS
BEGIN
    INSERT INTO Libro
    VALUES(@ISBN,@Titulo,@Descripcion,@Autor,@Anio,@Imagen_URL,@Id_Categoria);
END
GO

CREATE OR ALTER PROCEDURE ActualizarLibro
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
    UPDATE Libro SET
        ISBN=@ISBN,
        Descripcion=@Descripcion,
        Titulo=@Titulo,
        Autor=@Autor,
        Anio=@Anio,
        Imagen_URL=@Imagen_URL,
        Id_Categoria=@Id_Categoria
    WHERE Id_Libro=@Id_Libro;

    INSERT INTO Logs
    VALUES(GETDATE(),'Actualización','Se actualizó libro','Libro',@Id_Usuario);
END
GO


CREATE OR ALTER PROCEDURE ObtenerLibroPorId
    @Id INT
AS
BEGIN
    SELECT 
        Id_Libro,
        ISBN,
        Titulo,
        Descripcion,
        Autor,
        Anio,
        Imagen_URL,
        Id_Categoria
    FROM Libro
    WHERE Id_Libro = @Id;
END
GO

CREATE OR ALTER PROCEDURE EliminarLibro
(
    @Id_Libro INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar si existen ejemplares asociados
    IF EXISTS (
        SELECT 1 
        FROM Ejemplar 
        WHERE Id_Libro = @Id_Libro
    )
    BEGIN
        -- No se puede eliminar
        SELECT 0;
        RETURN;
    END

    -- Si no tiene ejemplares, se elimina (o se desactiva)
    DELETE FROM Libro
    WHERE Id_Libro = @Id_Libro;

    SELECT 1;
END


CREATE OR ALTER PROCEDURE EliminarCategoria
(
    @Id_Categoria INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar si hay libros asociados
    IF EXISTS (
        SELECT 1
        FROM Libro
        WHERE Id_Categoria = @Id_Categoria
    )
    BEGIN
        SELECT 0; -- No se puede eliminar
        RETURN;
    END

    DELETE FROM Categoria
    WHERE Id_Categoria = @Id_Categoria;

    SELECT 1; -- Eliminación exitosa
END

/* ===== EJEMPLARES ===== */

CREATE OR ALTER PROCEDURE ObtenerEjemplares
AS
BEGIN
    SELECT E.*, L.Titulo
    FROM Ejemplar E INNER JOIN Libro L ON E.Id_Libro = L.Id_Libro;
END
GO

CREATE OR ALTER PROCEDURE RegistrarEjemplar
    @CodigoEjemplar VARCHAR(50),
    @Id_Libro INT,
    @Estado VARCHAR(50),
    @Ubicacion VARCHAR(100),
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @i INT = 1;

    IF @Cantidad IS NULL OR @Cantidad <= 0
        RETURN;

    WHILE @i <= @Cantidad
    BEGIN
        INSERT INTO Ejemplar
        (
            CodigoEjemplar,
            Id_Libro,
            Estado,
            Ubicacion,
            Fecha_Registro
        )
        VALUES
        (
            CONCAT(@CodigoEjemplar, '-', @i),
            @Id_Libro,
            @Estado,
            @Ubicacion,
            GETDATE()
        );

        SET @i = @i + 1;
    END
END;

CREATE OR ALTER PROCEDURE ActualizarEjemplar
@Id INT,
@CodigoEjemplar VARCHAR(20),
@Estado VARCHAR(50),
@Ubicacion VARCHAR(100),
@Id_Libro INT
AS
BEGIN
    UPDATE Ejemplar SET
        CodigoEjemplar=@CodigoEjemplar,
        Estado=@Estado,
        Ubicacion=@Ubicacion,
        Id_Libro=@Id_Libro
    WHERE Id_Ejemplar=@Id;
END
GO

/* ===== RESERVAS / PRÉSTAMOS ===== */

USE BiblioSolaris;
GO

CREATE OR ALTER PROCEDURE ReservarLibro
(
    @Id_Libro INT,
    @Id_Usuario INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Id_Ejemplar INT;

    SELECT TOP 1 @Id_Ejemplar = Id_Ejemplar
    FROM Ejemplar
    WHERE Id_Libro = @Id_Libro
      AND Estado = 'Disponible'
    ORDER BY Id_Ejemplar;

    IF @Id_Ejemplar IS NULL
    BEGIN
        SELECT -1;
        RETURN;
    END

    INSERT INTO Movimiento
    (
        Tipo,
        Fecha,
        Fecha_Vencimiento,
        Id_Usuario,
        Id_Ejemplar,
        Id_Estado
    )
    VALUES
    (
        'Préstamo',
        @FechaInicio,
        @FechaFin,
        @Id_Usuario,
        @Id_Ejemplar,
        4
    );

    UPDATE Ejemplar
    SET Estado = 'Prestado'
    WHERE Id_Ejemplar = @Id_Ejemplar;

    SELECT 1;
END;
GO
----
CREATE OR ALTER PROCEDURE ObtenerReservasUsuario
@Id_Usuario INT
AS
BEGIN
    SELECT M.*, L.Titulo, L.Imagen_URL, S.Estado
    FROM Movimiento M
    INNER JOIN Ejemplar E ON M.Id_Ejemplar = E.Id_Ejemplar
    INNER JOIN Libro L ON E.Id_Libro = L.Id_Libro
    INNER JOIN Estado S ON M.Id_Estado = S.Id_Estado
    WHERE M.Id_Usuario=@Id_Usuario
    ORDER BY M.Fecha DESC;
END
GO

/* ===== FAQ ===== */

CREATE OR ALTER PROCEDURE ListarFAQActivas
AS
BEGIN
    SELECT Id_FAQ, Pregunta, Respuesta FROM FAQ WHERE Estado=1;
END
GO

CREATE OR ALTER PROCEDURE CrearFAQ
@Pregunta VARCHAR(255),
@Respuesta VARCHAR(MAX)
AS
BEGIN
    INSERT INTO FAQ(Pregunta,Respuesta) VALUES(@Pregunta,@Respuesta);
END
GO

/* ===== CONTACTO ===== */

CREATE OR ALTER PROCEDURE CrearContacto
@Nombre VARCHAR(100),
@Correo VARCHAR(150),
@Asunto VARCHAR(150),
@Mensaje VARCHAR(MAX)
AS
BEGIN
    INSERT INTO Contacto(Nombre,Correo,Asunto,Mensaje)
    VALUES(@Nombre,@Correo,@Asunto,@Mensaje);
END
GO


USE BiblioSolaris;
GO

/* ========================= CATEGORIAS ========================= */

CREATE OR ALTER PROCEDURE RegistrarCategoria
@Tipo VARCHAR(75)
AS
BEGIN
    INSERT INTO Categoria(Tipo) VALUES (@Tipo);
END
GO

CREATE OR ALTER PROCEDURE EditarCategoria
@Id_Categoria INT,
@Tipo VARCHAR(75)
AS
BEGIN
    UPDATE Categoria SET Tipo=@Tipo WHERE Id_Categoria=@Id_Categoria;
END
GO

CREATE OR ALTER PROCEDURE ObtenerCategorias
AS
BEGIN
    SELECT Id_Categoria, Tipo FROM Categoria;
END
GO

/* ========================= USUARIOS ADMIN ========================= */

CREATE OR ALTER PROCEDURE RegistroUsuarioAdmin
@Nombre VARCHAR(50),
@Apellidos VARCHAR(100),
@Identificacion VARCHAR(9),
@Correo VARCHAR(75),
@Contrasena VARCHAR(255),
@Telefono VARCHAR(15),
@Id_Rol INT
AS
BEGIN
    INSERT INTO Usuario
    VALUES(@Nombre,@Apellidos,@Identificacion,@Correo,@Contrasena,@Telefono,@Id_Rol,1);
END
GO

CREATE OR ALTER PROCEDURE ListarUsuarios
AS
BEGIN
    SELECT U.*, R.Tipo_Rol FROM Usuario U INNER JOIN Rol R ON U.Id_Rol=R.Id_Rol;
END
GO

CREATE OR ALTER PROCEDURE ObtenerUsuarioPorId
@Id_Usuario INT
AS
BEGIN
    SELECT U.*, R.Tipo_Rol FROM Usuario U INNER JOIN Rol R ON U.Id_Rol=R.Id_Rol
    WHERE Id_Usuario=@Id_Usuario;
END
GO

CREATE OR ALTER PROCEDURE ActualizarUsuarioAdmin
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
    UPDATE Usuario SET
        Nombre=@Nombre,
        Apellidos=@Apellidos,
        Identificacion=@Identificacion,
        Correo=@Correo,
        Telefono=@Telefono,
        Id_Rol=@Id_Rol,
        Estado=@Estado
    WHERE Id_Usuario=@Id_Usuario;
END
GO

CREATE OR ALTER PROCEDURE EliminarUsuario
@Id_Usuario INT
AS
BEGIN
    DELETE FROM Usuario WHERE Id_Usuario=@Id_Usuario;
END
GO

/* ========================= EJEMPLARES ========================= */

CREATE OR ALTER PROCEDURE ObtenerEjemplarPorId
@Id INT
AS
BEGIN
    SELECT E.*, L.Titulo FROM Ejemplar E
    INNER JOIN Libro L ON E.Id_Libro=L.Id_Libro
    WHERE Id_Ejemplar=@Id;
END
GO

CREATE OR ALTER PROCEDURE EliminarEjemplar
@Id INT
AS
BEGIN
    DELETE FROM Ejemplar WHERE Id_Ejemplar=@Id;
END
GO

/* ========================= RESERVAS ADMIN ========================= */

CREATE OR ALTER PROCEDURE ObtenerListaLibrosCliente
AS
BEGIN
    SELECT L.Id_Libro, L.Titulo, L.Autor, L.Imagen_URL, L.Descripcion,
           COUNT(E.Id_Ejemplar) AS Disponibles
    FROM Libro L
    LEFT JOIN Ejemplar E ON L.Id_Libro=E.Id_Libro AND E.Estado='Disponible'
    GROUP BY L.Id_Libro, L.Titulo, L.Autor, L.Imagen_URL, L.Descripcion;
END
GO

CREATE OR ALTER PROCEDURE CambiarEstadoReservaAdmin
@Id_Movimiento INT
AS
BEGIN
    UPDATE Movimiento SET Id_Estado=5 WHERE Id_Movimiento=@Id_Movimiento;
END
GO

CREATE OR ALTER PROCEDURE CancelarReserva
@Id_Movimiento INT
AS
BEGIN
    DECLARE @Id_Ejemplar INT;
    SELECT @Id_Ejemplar=Id_Ejemplar FROM Movimiento WHERE Id_Movimiento=@Id_Movimiento;
    UPDATE Movimiento SET Id_Estado=2 WHERE Id_Movimiento=@Id_Movimiento;
    UPDATE Ejemplar SET Estado='Disponible' WHERE Id_Ejemplar=@Id_Ejemplar;
END
GO

CREATE OR ALTER PROCEDURE ExtenderPrestamoCliente
@Fecha_Vencimiento DATETIME,
@Id_Movimiento INT
AS
BEGIN
    UPDATE Movimiento SET Fecha_Vencimiento=@Fecha_Vencimiento
    WHERE Id_Movimiento=@Id_Movimiento;
END
GO

CREATE OR ALTER PROCEDURE CambiarEstadoCompletado
@Id_Movimiento INT
AS
BEGIN
    UPDATE Movimiento SET Id_Estado=7 WHERE Id_Movimiento=@Id_Movimiento;
    UPDATE Ejemplar SET Estado='Disponible'
    WHERE Id_Ejemplar=(SELECT Id_Ejemplar FROM Movimiento WHERE Id_Movimiento=@Id_Movimiento);
END
GO

CREATE OR ALTER PROCEDURE ObtenerReservasAdmin
@EstadoFiltro INT = NULL
AS
BEGIN
    SELECT M.*, U.Nombre, U.Apellidos, E.Estado
    FROM Movimiento M
    INNER JOIN Usuario U ON M.Id_Usuario=U.Id_Usuario
    INNER JOIN Estado E ON M.Id_Estado=E.Id_Estado
    WHERE (@EstadoFiltro IS NULL OR M.Id_Estado=@EstadoFiltro);
END
GO

/* ========================= SANCIONES ========================= */

CREATE OR ALTER PROCEDURE UsuarioTieneSancionActiva
@Id_Usuario INT
AS
BEGIN
    SELECT COUNT(*) FROM Sancion
    WHERE Id_Usuario=@Id_Usuario AND Estado=1 AND Fecha_Finalizacion>GETDATE();
END
GO

CREATE OR ALTER PROCEDURE VerificarSancionesCumplidas
AS
BEGIN
    SELECT Id_Usuario FROM Sancion
    WHERE Estado=1 AND Fecha_Finalizacion<GETDATE();
END
GO

CREATE OR ALTER PROCEDURE InactivarSancionUsuario
@Id_Usuario INT
AS
BEGIN
    UPDATE Sancion SET Estado=0 WHERE Id_Usuario=@Id_Usuario;
END
GO

/* ========================= COMENTARIOS ========================= */

CREATE OR ALTER PROCEDURE AgregarComentario
(
    @Id_Usuario INT,
    @Id_Ejemplar INT,
    @Comentario VARCHAR(150),
    @Rating INT
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Comentario
    VALUES
    (
        GETDATE(),
        @Comentario,
        @Rating,
        @Id_Ejemplar,
        @Id_Usuario
    );
END

/* ========================= CONTACTO ========================= */

CREATE OR ALTER PROCEDURE ListarContactosAdmin
AS
BEGIN
    SELECT * FROM Contacto ORDER BY FechaEnvio DESC;
END
GO

CREATE OR ALTER PROCEDURE ListarContactosUsuario
@Correo VARCHAR(150)
AS
BEGIN
    SELECT * FROM Contacto WHERE Correo=@Correo ORDER BY FechaEnvio DESC;
END
GO

CREATE OR ALTER PROCEDURE ResponderContacto
@Id_Contacto INT,
@Respuesta VARCHAR(MAX)
AS
BEGIN
    UPDATE Contacto SET Respuesta=@Respuesta, Estado=0, FechaRespuesta=GETDATE()
    WHERE Id_Contacto=@Id_Contacto;
END
GO

/* ========================= REPORTES ========================= */

CREATE OR ALTER PROCEDURE ObtenerReservasActivas
AS
BEGIN
    SELECT M.Id_Movimiento, M.Fecha, M.Fecha_Vencimiento, L.Titulo
    FROM Movimiento M
    INNER JOIN Ejemplar E ON M.Id_Ejemplar=E.Id_Ejemplar
    INNER JOIN Libro L ON E.Id_Libro=L.Id_Libro
    WHERE M.Id_Estado=5;
END
GO

ALTER PROCEDURE ObtenerReservasActivas
    @Id_Usuario INT
AS
BEGIN
    SELECT 
        r.Id_Movimiento,
        r.FechaReserva,
        r.FechaVencimiento,
        e.Estado,
        l.Titulo,
        l.Imagen_URL
    FROM Reserva r
    INNER JOIN Ejemplar e ON r.Id_Ejemplar = e.Id_Ejemplar
    INNER JOIN Libro l ON e.Id_Libro = l.Id_Libro
    WHERE r.Id_Usuario = @Id_Usuario
      AND r.Estado = 'Activa'
END

CREATE OR ALTER PROCEDURE ObtenerLibrosConMasCantidadMovimientos
AS
BEGIN
    SELECT TOP 5 L.Titulo, COUNT(*) AS CantidadMovimientos
    FROM Movimiento M
    INNER JOIN Ejemplar E ON M.Id_Ejemplar=E.Id_Ejemplar
    INNER JOIN Libro L ON E.Id_Libro=L.Id_Libro
    GROUP BY L.Titulo
    ORDER BY CantidadMovimientos DESC;
END
GO



/* ****************************************************************************************************
   *********************************** REPORTES DE DATOS RELEVANTES ***********************************
   **************************************************************************************************** */
-- SP OBTENER RESERVAS ACTIVAS -------------------------------------------------------------------------------------
CREATE PROCEDURE ObtenerReservasActivas
AS
BEGIN
 SELECT M.Id_Movimiento, M.Tipo, M.Fecha, M.Fecha_Vencimiento, M.Id_Estado, CONCAT(U.Nombre, ' ',U.Apellidos) AS Nombre, L.Titulo
 FROM Movimiento M 
    INNER JOIN Usuario U ON M.Id_Usuario = U.Id_Usuario 
 INNER JOIN Libro L ON M.Id_Ejemplar = L.Id_Libro
    INNER JOIN Estado E ON M.Id_Estado = E.Id_Estado WHERE E.Estado IN 
 -- AND M.Id_Estado = 1
END;
--EXEC ObtenerReservasActivas;

-- SP OBTENER LIBROS DEL TOP 5 -------------------------------------------------------------------------------------
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
--EXEC ObtenerLibrosConMasCantidadMovimientos;

-- SP OBTENER USUARIOS CON MÁS SANCIONES -------------------------------------------------------------------------------------
CREATE PROCEDURE ObtenerUsuariosConMasSanciones
AS
BEGIN
    SELECT 
        U.Id_Usuario,
        CONCAT(U.Nombre, ' ', U.Apellidos) AS Usuario,
        COUNT(S.Id_Sancion) AS TotalSanciones,
        MIN(S.Fecha_Finalizacion) AS ProximaFechaFin
    FROM Sancion S
    INNER JOIN Movimiento M ON S.Id_Movimiento = M.Id_Movimiento
    INNER JOIN Usuario U ON M.Id_Usuario = U.Id_Usuario
    WHERE S.Estado = 1
    GROUP BY U.Id_Usuario, U.Nombre, U.Apellidos
    ORDER BY TotalSanciones DESC;
END;

/* ****************************************************************************************************
   ********************************** MODULO DE BITACORA & AUDITLOGS **********************************
   **************************************************************************************************** */
-- SP PARA REGISTRAR BITACORA
CREATE PROCEDURE RegistrarLog
    @Id_Usuario INT = NULL,
    @Modulo_Afectado VARCHAR(50),
    @Tipo_Accion VARCHAR(25),
    @Descripcion_Accion VARCHAR(150)
AS
BEGIN
    INSERT INTO Logs (Fecha, Tipo_Accion, Descripcion_Accion, Modulo_Afectado, Id_Usuario)
    VALUES (GETDATE(), @Tipo_Accion, @Descripcion_Accion, @Modulo_Afectado, @Id_Usuario);
END;
GO

-- SP PARA OBTENER BITACORA
CREATE PROCEDURE ObtenerLogs
AS
BEGIN
    SELECT 
        L.Id_Log,
        L.Fecha,
        L.Tipo_Accion,
        L.Descripcion_Accion,
        L.Modulo_Afectado,
        CONCAT(U.Nombre, ' ', U.Apellidos) AS NombreUsuario
    FROM Logs L
    LEFT JOIN Usuario U ON L.Id_Usuario = U.Id_Usuario
    ORDER BY L.Fecha DESC;
END;
GO


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

    SELECT L.Titulo, U.Correo FROM Ejemplar E INNER JOIN Libro L ON E.Id_Libro = L.Id_Libro 
 INNER JOIN Usuario U ON U.Id_Usuario = @Id_Usuario
 WHERE E.Id_Ejemplar = @Id_Ejemplar;
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

------------------OBTENER USUARIOS PARA ADMIN RESERVAR, MISMO FIN --------------------------------------

CREATE OR ALTER PROCEDURE ListarUsuariosAdmin
AS
BEGIN
 SELECT Id_Usuario, Nombre, Apellidos, Correo, Telefono, U.Id_Rol, Estado, Identificacion, R.Tipo_Rol
 FROM Usuario U INNER JOIN Rol R ON U.Id_Rol = R.Id_Rol WHERE U.Id_Rol = 2
END

-----------------------------RESERVAS-------------------------------------------------------------------

CREATE OR ALTER PROCEDURE ObtenerReservasAdmin
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
 ORDER BY M.Id_Movimiento DESC;
END;

-------------------------------------NUEVO RESERVAS LIBROS--------------------------------------------

CREATE OR ALTER PROCEDURE ReservarLibro
    @Id_Libro INT,--
    @Fecha DATETIME,
    @Fecha_Vencimiento DATETIME,
    @Id_Usuario INT
AS
BEGIN
    DECLARE @Id_Ejemplar INT;
 DECLARE @Titulo VARCHAR(200);

 SELECT TOP 1 @Id_Ejemplar = E.Id_Ejemplar, @Titulo = L.Titulo
 FROM Ejemplar E INNER JOIN Libro L ON E.Id_Libro = L.Id_Libro WHERE E.Id_Libro = @Id_Libro 
 AND Estado = 'Disponible' ORDER BY Id_Ejemplar;

 IF @Id_Ejemplar IS NULL
 BEGIN 
  SELECT -1 AS Resultado, NULL AS Titulo
  RETURN;
 END

    INSERT INTO Movimiento (Tipo, Fecha, Fecha_Vencimiento, Id_Estado, Id_Usuario, Id_Ejemplar)
    VALUES ('Préstamo', @Fecha, @Fecha_Vencimiento, 4, @Id_Usuario, @Id_Ejemplar);

 UPDATE Ejemplar SET Estado = 'Prestado' WHERE Id_Ejemplar = @Id_Ejemplar;

    SELECT 1 AS Resultado, @Titulo AS Titulo;
END;


----------------------ACTUALIZAR CONTRASEÑA--------------------
CREATE PROCEDURE ActualizarContrasena
    @Id_Usuario INT,
    @Contrasena VARCHAR(255)
AS
BEGIN
    UPDATE Usuario
    SET Contrasena = @Contrasena
    WHERE Id_Usuario = @Id_Usuario;
END;
GO


