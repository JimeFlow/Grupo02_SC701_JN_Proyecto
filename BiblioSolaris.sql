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
