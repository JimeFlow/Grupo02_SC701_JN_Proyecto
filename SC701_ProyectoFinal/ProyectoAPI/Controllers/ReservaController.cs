using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using ProyectoAPI.Services;
using System.Text;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : Controller
    {
        private readonly IConfiguration _configuration;

        public ReservaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Crear")]
        public IActionResult crearReserva([FromBody] ReservaLibroModel reserva)
        {
            try
            {
                // Lógica para crear una reserva en la base de datos
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    SqlCommand command = new SqlCommand("CrearReserva", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id_Usuario", reserva.UsuarioId);
                    command.Parameters.AddWithValue("@Id_Libro", reserva.LibroId);
                    command.Parameters.AddWithValue("@Fecha_Reserva", reserva.FechaReserva);
                    command.Parameters.AddWithValue("@Fecha_Vencimiento", reserva.FechaVencimiento);
                    command.Parameters.AddWithValue("@Id_Estado", reserva.EstadoReserva);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                // Obtener el correo del usuario para enviar la notificación
                string correoUsuario = "";
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    SqlCommand command = new SqlCommand("SELECT Correo FROM Usuario WHERE Id_Usuario = @Id", connection);
                    command.Parameters.AddWithValue("@Id", reserva.UsuarioId);

                    connection.Open();
                    correoUsuario = command.ExecuteScalar().ToString() ?? "";
                    connection.Close();
                }

                if (string.IsNullOrEmpty(correoUsuario))
                {
                    return BadRequest("No se pudo obtener el correo del usuario.");
                }

                // Preparar el contenido del correo utilizando la plantilla
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "PlantillasCorreo", "NotificacionReserva.html");
                string html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);
                html = html.Replace("{{Usuario}}", "Nombre");
                html = html.Replace("{{Libro}}", "Titulo");
                html = html.Replace("{{FechaReserva}}", reserva.FechaReserva.ToString("dd/MM/yyyy"));
                html = html.Replace("{{FechaVencimiento}}", reserva.FechaVencimiento.ToShortDateString());

                // REVISAR
                //CorreoService _correoService = new CorreoService(_configuration);
                //_correoService.EnviarCorreo(correoUsuario, "Confirmación de Reserva de Libro", html);
                


                //using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                //{
                    //SqlCommand command = new SqlCommand("RegistrarCorreoReserva", connection);
                   // command.CommandType = System.Data.CommandType.StoredProcedure;

                    //command.Parameters.AddWithValue("@Id_Usuario", reserva.UsuarioId);
                    //command.Parameters.AddWithValue("@Descripcion", "Correo de confirmacion enviado por reserva");

                    //connection.Open();
                    //command.ExecuteNonQuery();
                    //connection.Close();
                //}
                

                return Ok(new { mensaje = "Reserva creada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear la reserva.", detalle = ex.Message });
            }

        }

        [HttpGet("Usuario/{idUsuario}")]
        public IActionResult ObtenerReservasDeUsuario(int idUsuario)
        {
            var reservas = new List<object>();

            try
            {
                using (var conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    conexion.Open();

                    // Consulta SQL para traer las reservas del usuario
                    string query = @"
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
                WHERE M.Id_Usuario = @usuario
                ADN M.Tipo = 'Reserva'
                AND M-Id_Estado <> 5
                ORDER BY M.Fecha DESC";

                    var cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@usuario", idUsuario);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        reservas.Add(new
                        {
                            Id = Convert.ToInt32(reader["Id_Movimiento"]),
                            LibroId = Convert.ToInt32(reader["Id_Libro"]),
                            FechaReserva = Convert.ToDateTime(reader["Fecha"]),
                            FechaVencimiento = Convert.ToDateTime(reader["Fecha_Vencimiento"]),
                            Titulo = reader["Titulo"].ToString(),
                            Imagen = reader["Imagen_URL"].ToString(),
                            Estado = reader["Estado"].ToString()
                        });
                    }
                }

                return Ok(reservas); // Se envía la lista al MVC
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reservas", detalle = ex.Message });
            }


        }

        [HttpDelete("Cancelar/{idReserva}")]
        public IActionResult CancelarReserva(int idReserva)
        {
            try
            {
                using (var conexion =
                       new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    conexion.Open();

                    string query = @"
                -- Cancelar la reserva
                UPDATE Movimiento
                SET Id_Estado = 5
                WHERE Id_Movimiento = @idReserva;

                -- Volver a poner el libro como Disponible
                UPDATE Libro
                SET Id_Estado = 1
                WHERE Id_Libro = (
                    SELECT Id_Libro 
                    FROM Movimiento 
                    WHERE Id_Movimiento = @idReserva
                );";

                    var cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@idReserva", idReserva);
                    cmd.ExecuteNonQuery();
                }

                return Ok(new { mensaje = "Reserva cancelada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al cancelar reserva", detalle = ex.Message });
            }
        }
    }

}

    
    




