using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [AllowAnonymous]
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

                resultado.Token = GenerarToken(resultado.Id_Usuario, resultado.Nombre, resultado.Id_Rol);
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

        //generar token JWT
        private string GenerarToken(int usuarioId, string nombre, int rol)
        {
            var key = _configuration["Valores:KeyJWT"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", usuarioId.ToString()),
                new Claim("nombre", nombre),
                new Claim("rol", rol.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
