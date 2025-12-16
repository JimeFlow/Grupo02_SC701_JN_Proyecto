using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ProyectoAPI.Controllers
{
    [Authorize(Roles = "1")] // solo admin
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BitacoraController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("ObtenerBitacora")]
        public IActionResult ObtenerBitacora()
        {
            using var context = new SqlConnection(
                _configuration["ConnectionStrings:BDConnection"]);

            var resultado = context.Query<BitacoraResponseModel>(
                "ObtenerBitacora",
                commandType: CommandType.StoredProcedure
            );

            return Ok(resultado);
        }
    }
}
