using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FAQController : Controller
    {
        private readonly IConfiguration _configuration;

        public FAQController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // CLIENTE
        [HttpGet("Activas")]
        public IActionResult ListarFAQActivas()
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);
            var faqs = db.Query<FAQModel>("ListarFAQActivas",
                commandType: CommandType.StoredProcedure);

            return Ok(faqs);
        }

        // ADMIN
        [Authorize]
        [HttpGet("Admin")]
        public IActionResult ListarFAQAdmin()
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);
            var faqs = db.Query<FAQModel>("ListarFAQAdmin",
                commandType: CommandType.StoredProcedure);

            return Ok(faqs);
        }

        [Authorize]
        [HttpPost("Crear")]
        public IActionResult CrearFAQ([FromBody] FAQRequestModel model)
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            db.Execute(
                "CrearFAQ",
                new { model.Pregunta, model.Respuesta, model.Estado},
                commandType: CommandType.StoredProcedure);

            return Ok();
        }

        [Authorize]
        [HttpPut("CambiarEstado")]
        public IActionResult CambiarEstado(int idFAQ, bool estado)
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            db.Execute("CambiarEstadoFAQ",
                new { Id_FAQ = idFAQ, Estado = estado },
                commandType: CommandType.StoredProcedure);

            return Ok();
        }
    }
}
   
