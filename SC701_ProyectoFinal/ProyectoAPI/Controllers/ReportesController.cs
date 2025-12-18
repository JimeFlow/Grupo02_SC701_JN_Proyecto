using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : Controller
    {
        private readonly IConfiguration _configuration;
        public ReportesController(IConfiguration configuration) => _configuration = configuration;


        [HttpGet("reservas-activas")]
        public async Task<IActionResult> ReservasActivas()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Biblio Solaris"));
            var datos = await connection.QueryAsync<Models.ReservasActivasModel>(
                "ObtenerReservasActivas", commandType: CommandType.StoredProcedure);
            return Ok(datos);
        }

        [HttpGet("top-libros")]
        public async Task<IActionResult> TopLibros()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Biblio Solaris"));
            var datos = await connection.QueryAsync<Models.TopLibroModel>(
                "ObtenerLibrosConMasCantidadMovimientos", commandType: CommandType.StoredProcedure);
            return Ok(datos);
        }

        [HttpGet("usuario-mas-sancionado")]
        public async Task<IActionResult> UsuarioMasSancionado()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Biblio Solaris"));
            var datos = await connection.QueryAsync<Models.UsuarioMasSancionadoModel>(
                "ObtenerUsuariosConMasSanciones", commandType: CommandType.StoredProcedure);
            return Ok(datos);

        }
    }

}
