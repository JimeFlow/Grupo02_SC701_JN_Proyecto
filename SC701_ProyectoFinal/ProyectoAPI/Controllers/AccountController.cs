using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("IniciarSesion")]
        public IActionResult IniciarSesion(InicioSesionRequestModel user)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@Correo", user.Correo);

                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ObtenerUsuarioPorCorreo", parametros);

                if (resultado == null)
                    return NotFound(new { mensaje = "El usuario no fue encontrado o se encuentra inactivo" });


                bool validPassword = BCrypt.Net.BCrypt.Verify(user.Contrasena, resultado.Contrasena);

                if (!validPassword) return Unauthorized(new { mensaje = "Contraseña incorrecta" });


                return Ok(resultado);                

            }
        }

        [HttpPost]
        [Route("Registrarse")]
        public IActionResult Registrarse(RegistroUsuarioRequestModel user)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                string hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Contrasena);

                parametros.Add("@Nombre", user.Nombre);
                parametros.Add("@Apellidos", user.Apellidos);
                parametros.Add("@Identificacion", user.Identificacion);
                parametros.Add("@Correo", user.Correo);
                parametros.Add("@Contrasena", hashPassword);
                parametros.Add("@Telefono", user.Telefono);

                var resultado = context.Execute("RegistroUsuario", parametros);

                return Ok(resultado);
            }
        }
    }
}
