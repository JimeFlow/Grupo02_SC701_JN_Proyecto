using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

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

        [HttpPost]
        public IActionResult crearReserva([FromBody] ReservaLibroModel reserva)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BiblioSolaris")))
            {
                SqlCommand command = new SqlCommand("CrearReserva", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("Id_Usuario", reserva.UsuarioId);
                command.Parameters.AddWithValue("Id_Libro", reserva.LibroId);
                command.Parameters.AddWithValue("Fecha_Reserva", reserva.FechaReserva);
                command.Parameters.AddWithValue("Fecha_Vencimiento", reserva.FechaVencimiento);
                command.Parameters.AddWithValue("Id_Estado", reserva.EstadoReserva);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            return Ok("Reserva creada exitosamente.");
        }
    }
}
