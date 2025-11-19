using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Models;
using Microsoft.Data.SqlClient;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EstadoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ObtenerEstados()
        {
            var lista = new List<EstadoModel>();

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
            {
                conexion.Open();
                string query = "SELECT Id_Estado, Estado FROM Estado";
                SqlCommand cmd = new SqlCommand(query, conexion);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new EstadoModel
                    {
                        Id_Estado = Convert.ToInt32(reader["Id_Estado"]),
                        Estado = reader["Estado"].ToString()
                    });
                }
            }

            return Ok(lista);
        }
    }
}
