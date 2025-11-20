using System.Data;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using ProyectoAPI.Services;

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
                // Logica para crear una reserva en la base de datos
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

                return Ok(new { mensaje = "¡Reserva creada exitosamente!" });
            }            

        

        [HttpGet("Usuario/{idUsuario}")]
        public IActionResult ObtenerReservasDeUsuario(int idUsuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", idUsuario);

                var resultado = context.Query<dynamic>(
                    "ObtenerReservasUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }


        }

        [HttpDelete("Cancelar/{idReserva}")]
        public IActionResult CancelarReserva(int idReserva)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Movimiento", idReserva);

                context.Execute("CancelarReserva", parametros);
            }

            return Ok(new { mensaje = "Reserva cancelada exitosamente." });
        }
    }

}

    
    




