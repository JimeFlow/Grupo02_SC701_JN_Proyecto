using System.Data;
using Dapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ErrorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("RegistrarError")]
        public IActionResult RegistrarError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", 0);
                parametros.Add("@MensajeError", exception?.Error.Message);
                parametros.Add("@OrigenError", exception?.Path);

                context.Execute(
                    "RegistrarError",
                    parametros,
                    commandType: CommandType.StoredProcedure
                    );
            }

            return StatusCode(500, "Se presentó una excepción en nuestro servicio");
        }
    }
}
