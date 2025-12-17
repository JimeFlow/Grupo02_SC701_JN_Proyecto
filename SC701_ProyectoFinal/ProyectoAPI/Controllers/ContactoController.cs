using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ContactoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // CLIENTE – enviar contacto
        [AllowAnonymous]
        [HttpPost("Crear")]
        public IActionResult Crear([FromBody] ContactoRequestModel model)
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            db.Execute("dbo.CrearContacto", model, commandType: CommandType.StoredProcedure);
            return Ok();
        }

        // USUARIO – ver sus consultas
        [Authorize]
        [HttpGet("Usuario")]
        public IActionResult ListarPorUsuario([FromQuery] string correo)
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var datos = db.Query<ContactoModel>(
                "dbo.ListarContactosUsuario",
                new { Correo = correo },
                commandType: CommandType.StoredProcedure
            );

            return Ok(datos);
        }

        // ADMIN – listar contactos
        [Authorize]
        [HttpGet("Admin")]
        public IActionResult ListarAdmin()
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var contactos = db.Query<ContactoModel>(
                "ListarContactosAdmin",
                commandType: CommandType.StoredProcedure
            );

            return Ok(contactos);
        }

        [Authorize]
        [HttpPost("Responder")]
        public IActionResult Responder([FromBody] ResponderContactoRequest model)
        {
            using var db = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            db.Execute(
                "ResponderContacto",
                model,
                commandType: CommandType.StoredProcedure
            );

            return Ok();
        }
    }
}

