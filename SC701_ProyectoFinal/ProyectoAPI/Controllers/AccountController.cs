using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ProyectoAPI.Models;
using Utils;

namespace ProyectoAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public AccountController(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        #region LOGIN
        [HttpPost]
        [Route("IniciarSesion")]
        [AllowAnonymous]
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

                resultado.Token = GenerarToken(resultado.Id_Usuario, resultado.Nombre, resultado.Id_Rol, resultado.Correo);
                return Ok(resultado);                

            }
        }
        #endregion

        #region REGISTER

        [HttpPost]
        [Route("Registrarse")]
        [AllowAnonymous]
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

                var resultado = context.QueryFirst<int>("RegistroUsuario", 
                    parametros,
                    commandType: CommandType.StoredProcedure
                    );
                if (resultado == 0) return BadRequest("El correo o el número de teléfono ya existe");

                return Ok("Usuario registrado con éxito");
            }
        }
        #endregion

        #region RECUPERAR ACCESO

        [HttpGet]
        [Route("RecuperarAcceso")]
        [AllowAnonymous]
        public IActionResult ValidarUsuario([Required] string Correo)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var helper = new Helper();
                var parametros = new DynamicParameters();
                parametros.Add("@Correo", Correo);

                var user = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ValidarUsuario", parametros);

                if (user != null)
                {
                    var contrasenaGenerada = GenerarContrasena();

                    var contrasenaCifrada = BCrypt.Net.BCrypt.HashPassword(contrasenaGenerada);

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@Id_Usuario", user.Id_Usuario);
                    parametrosActualizar.Add("@Contrasena", contrasenaCifrada);

                    var resultadoActualizar = context.Execute("ActualizarContrasena", parametrosActualizar);

                    if (resultadoActualizar > 0)
                    {

                        var ruta = Path.Combine(_environment.ContentRootPath, "PlantillasCorreo", "Recuperacion.html");
                        var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                        html = html.Replace("{{Nombre}}", user.Nombre);
                        html = html.Replace("{{Contrasena}}", contrasenaGenerada);

                        helper.EnviarCorreo("Recuperar Acceso", html, user.Correo);

                        return Ok(user);
                    }
                }

                return NotFound();
            }
        }
        #endregion

        #region ACTUALIZAR SEGURIDAD

        [HttpPut]
        [Route("ActualizarSeguridad")]
        [Authorize]
        public IActionResult ActualizarSeguridad(SeguridadRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                string hashPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
                var parametros = new DynamicParameters();
                parametros.Add("Id_Usuario", usuario.Id_Usuario);
                parametros.Add("Contrasena", hashPassword);

                var resultado = context.Execute("ActualizarContrasena", parametros);
                return Ok(resultado);
            }
        }
        #endregion

        #region ACTUALIZAR PERFIL

        [HttpPut]
        [Route("ActualizarPerfil")]
        [Authorize]
        public IActionResult ActualizarPerfil(PerfilRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("Id_Usuario", usuario.Id_Usuario);
                parametros.Add("Nombre", usuario.Nombre);
                parametros.Add("Apellidos", usuario.Apellidos);
                parametros.Add("Identificacion", usuario.Identificacion);
                parametros.Add("Correo", usuario.Correo);
                parametros.Add("Telefono", usuario.Telefono);

                var resultado = context.QueryFirst<int>("ActualizarPerfil", parametros);
                if(resultado == 0)
                {
                    return BadRequest("Ya existe un usuario con el correo, número de teléfono o identificación");
                }
                return Ok("Perfil actualizado correctamente");
            }
        }
        #endregion

        #region METODOS PRIVADOS

        private string GenerarContrasena()
        {
            int longitud = 8;
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder resultado = new();

            using var rng = RandomNumberGenerator.Create();
            byte[] buffer = new byte[1];

            while (resultado.Length < longitud)
            {
                rng.GetBytes(buffer);
                int valor = buffer[0] % caracteres.Length;
                resultado.Append(caracteres[valor]);
            }

            return resultado.ToString();
        }

        

        //generar token JWT
        private string GenerarToken(int usuarioId, string nombre, int rol, string correo)
        {
            var key = _configuration["Valores:KeyJWT"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", usuarioId.ToString()),
                new Claim("nombre", nombre),
                new Claim("rol", rol.ToString()),
                new Claim("correo", correo)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
