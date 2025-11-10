using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsuarioController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("RegistrarUsuario")]
        public IActionResult RegistrarUsuarioAdmin(RegistroUsuarioAdminRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Nombre", usuario.Nombre);
                parametros.Add("@Apellidos", usuario.Apellidos);
                parametros.Add("@Identificacion", usuario.Identificacion);
                parametros.Add("@Correo", usuario.Correo);
                parametros.Add("@Contrasena", usuario.Contrasena);
                parametros.Add("@Telefono", usuario.Telefono);
                parametros.Add("@Id_Rol", usuario.Id_Rol);

                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("RegistroUsuarioAdmin", parametros);
                return Ok(resultado!.Id_Usuario);
            }
        }

        [HttpGet]
        [Route("ListaUsuarios")]
        public IActionResult ListarUsuarios()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                var resultado = context.Query<DatosUsuarioResponseModel>("ListarUsuarios", parametros);
                return Ok(resultado);
            }
        }

        [HttpPut]
        [Route("EditarUsuario")]
        public IActionResult EditarUsuarioAdmin(EditarUsuarioRequestModel usuario)
        {
            return Ok();
        }
    }
}
