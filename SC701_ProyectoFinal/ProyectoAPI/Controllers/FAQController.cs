using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FAQController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // LISTAR SOLO FAQ ACTIVOS
        [HttpGet]
        public IActionResult ObtenerFAQ()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var resultado = context.Query<FAQModel>(
                    "ListarFAQActivos",
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

    }

}
