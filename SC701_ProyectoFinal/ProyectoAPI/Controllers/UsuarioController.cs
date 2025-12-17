using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IHostEnvironment _environment;

        public UsuarioController(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost]
        [Route("RegistrarUsuario")]
        public IActionResult RegistrarUsuarioAdmin(RegistroUsuarioAdminRequestModel usuario)
        {           
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var generateNewPass = GenerarContrasena();
                string hashPassword = BCrypt.Net.BCrypt.HashPassword(generateNewPass);
                usuario.Contrasena = hashPassword;
                var parametros = new DynamicParameters();
                parametros.Add("@Nombre", usuario.Nombre);
                parametros.Add("@Apellidos", usuario.Apellidos);
                parametros.Add("@Identificacion", usuario.Identificacion);
                parametros.Add("@Contrasena", usuario.Contrasena);
                parametros.Add("@Correo", usuario.Correo);
                parametros.Add("@Telefono", usuario.Telefono);
                parametros.Add("@Id_Rol", usuario.Id_Rol);

                var resultado = context.QueryFirstOrDefault<int>("RegistroUsuarioAdmin", parametros);
                if(resultado > 0)
                {
                    //Enviar Correo
                    var ruta = Path.Combine(_environment.ContentRootPath,"PlantillasCorreo", "NuevoUsuarioAdmin.html");
                    var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                    html = html.Replace("{{Nombre}}", usuario.Nombre);
                    html = html.Replace("{{Contrasena}}", generateNewPass);
                    html = html.Replace("{{UrlAcceso}}", "https://localhost:7163");

                    EnviarCorreo("Creación de usuario", html, usuario.Correo);
                    return Ok(resultado);
                }
                return Ok(resultado);
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
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("Id_Usuario", usuario.Id_Usuario);
                parametros.Add("Nombre", usuario.Nombre);
                parametros.Add("Apellidos", usuario.Apellidos);
                parametros.Add("Identificacion", usuario.Identificacion);
                parametros.Add("Correo", usuario.Correo);
                parametros.Add("Telefono", usuario.Telefono);
                parametros.Add("Id_Rol", usuario.Id_Rol);
                parametros.Add("Estado", usuario.Estado);

                var resultado = context.Execute("ActualizarUsuarioAdmin", parametros);
                return Ok(resultado);
            }
        }

        [HttpDelete]
        [Route("EliminarUsuario/{userId}")]
        public IActionResult EliminarUsuario(int userId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("Id_Usuario", userId);

                var resultado = context.Execute("EliminarUsuario", parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerUsuario/{id}")]
        public IActionResult ObtenerUsuario(int id)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("Id_Usuario", id);

                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ObtenerUsuarioPorId", parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ListarRoles")]
        public IActionResult ListarRoles()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                var resultado = context.Query<RolResponseModel>("ListarRoles", parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ListaUsuariosAdmin")]
        public IActionResult ListarUsuariosAdmin()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                var resultado = context.Query<DatosUsuarioResponseModel>("ListarUsuariosAdmin", parametros);
                return Ok(resultado);
            }
        }

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

        private void EnviarCorreo(string subject, string body, string destinatario)
        {
            var correoSMTP = _configuration["Valores:CorreoSMTP"]!;
            var contrasennaSMTP = _configuration["Valores:ContrasenaSMTP"]!;

            if (string.IsNullOrEmpty(contrasennaSMTP))
                return;

            var mensaje = new MailMessage
            {
                From = new MailAddress(correoSMTP),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoSMTP, contrasennaSMTP),
                EnableSsl = true
            };

            smtp.Send(mensaje);
        }
        #endregion

    }
}
