using System.Data;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using ProyectoAPI.Services;
using Utils;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public ReservaController(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost("Crear")]
        public IActionResult crearReserva([FromBody] ReservaLibroModel reserva)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var helper = new Helper();
                var parametros = new DynamicParameters();
                parametros.Add("Id_Libro", reserva.Id_Libro);
                parametros.Add("Id_Usuario", reserva.Id_Usuario);
                parametros.Add("FechaInicio", reserva.FechaReserva);
                parametros.Add("FechaFin", reserva.FechaVencimiento);


                var resultado = context.Execute("ActualizarUsuarioAdmin", parametros);
                if (resultado > 0)
                {
                    int consecutivoUsuario = int.TryParse(HttpContext.User.FindFirst("id")?.Value, out var id) ? id
             : 0;
                    string? nombreUsuario = HttpContext.User.FindFirst("nombre")?.Value;
                    string? correoUsuario = HttpContext.User.FindFirst("correo")?.Value;
                    if (nombreUsuario != null && correoUsuario != null)
                    {
                        //Enviar Correo
                        var ruta = Path.Combine(_environment.ContentRootPath, "PlantillasCorreo", "NotificacionReserva.html");
                        var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                        html = html.Replace("{{Usuario}}", nombreUsuario);
                        html = html.Replace("{{Libro}}", reserva.Titulo);
                        html = html.Replace("{{FechaReserva}}", reserva.FechaReserva.ToString("dd/MM/yyyy"));
                        html = html.Replace("{{FechaVencimiento}}", reserva.FechaVencimiento.ToShortDateString());

                        helper.EnviarCorreo("Confirmación de Reserva de Libro", html, correoUsuario);
                        return Ok(resultado);
                    }


                }
                return Ok(resultado);
            }

        }

        [HttpGet]
        [Route("ObtenerReservas")]
        public IActionResult ObtenerReservas(int Id_Usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", Id_Usuario);

                var resultado = context.Query<ReservaResponseModel>("ObtenerReservasUsuario", parametros);
                return Ok(resultado);
            }
        }

        [HttpPut]
        [Route("CambiarEstadoReserva")]
        public IActionResult CambiarEstadoReservaAdmin(CambiarEstadoReservaRequestModel model)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Movimiento", model.Id_Movimiento);

                var resultado = context.Execute("CambiarEstadoReservaAdmin", parametros);
                return Ok(resultado);
            }
        }

        [HttpPut]
        [Route("ExtenderPrestamo")]
        public IActionResult ExtenderPrestamo(ExtenderPrestamoRequestModel reserva)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Movimiento", reserva.Id_Movimiento);
                parametros.Add("@Fecha_Vencimiento", reserva.Fecha_Vencimiento);

                var resultado = context.Execute("ExtenderPrestamoCliente", parametros);
                return Ok(resultado);
            }
        }

        [HttpPut]
        [Route("CancelarReserva")]
        public IActionResult CancelarReserva(CambiarEstadoReservaRequestModel model)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Movimiento", model.Id_Movimiento);

                var resultado = context.Execute("CancelarReserva", parametros);
                return Ok(resultado);
            }

        }

        [HttpPut]
        [Route("CancelarEstadoCancelado")]
        public IActionResult CambiarEstadoCancelado(CambiarEstadoReservaRequestModel model)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Movimiento", model.Id_Movimiento);

                var resultado = context.Execute("CambiarEstadoCancelado", parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerReservasAdmin")]
        public IActionResult ObtenerReservasPendientesAdmin(int? estado)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                if(estado != null)
                {
                    parametros.Add("EstadoFiltro", estado);
                }

                var resultado = context.Query<ReservaResponseModel>("ObtenerReservasAdmin", parametros);
                return Ok(resultado);
            }
        }



        [HttpGet("Usuario/{idUsuario}")]
        public IActionResult ObtenerReservasDeUsuario(int idUsuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", idUsuario);

                var resultado = context.Query<dynamic>( //id_Libro
                    "ObtenerReservasUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }


        }


    }

}







