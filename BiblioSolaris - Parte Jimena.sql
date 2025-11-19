USE BiblioSolaris;
GO

/* ****************************************************************************************************
   ********************************* MODIFICACIONES DE LA BD - TABLAS *********************************
   **************************************************************************************************** */

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

/* ****************************************************************************************************
   ************************************ PROCEDIMIENTOS ALMACENADOS ************************************
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