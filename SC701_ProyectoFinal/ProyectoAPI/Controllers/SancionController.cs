using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SancionController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public SancionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        [Route("RegistrarSancion")]
        public IActionResult RegistrarSancion(SancionRequestModel model)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", model.Id_Usuario);

                var resultado = context.ExecuteScalar<int>(
                    "RegistrarSancion",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpPut]
        [Route("InactivarSancionUsuario")]
        public IActionResult InactivarSancionUsuario(SancionRequestModel model)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", model.Id_Usuario);

                var resultado = context.Execute(
                    "InactivarSancionUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado); 
            }
        }

        [HttpGet]
        [Route("ObtenerSancionesCumplidas")]
        public IActionResult ObtenerSancionesCumplidas()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var resultado = context.Query<int>(
                    "VerificarSancionesCumplidas",
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado.ToList());
            }
        }

        [HttpGet]
        [Route("UsuarioTieneSancion")]
        public IActionResult UsuarioTieneSancionActiva()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                int consecutivoUsuario = int.TryParse(HttpContext.User.FindFirst("id")?.Value, out var id) ? id
             : 0;
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", consecutivoUsuario);
                int tieneSancion = context.ExecuteScalar<int>(
                        "UsuarioTieneSancionActiva",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                return Ok(tieneSancion);
            }            
        }



    }
}
