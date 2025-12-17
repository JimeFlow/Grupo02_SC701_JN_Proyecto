using System.Data;
using System.Text;
using Azure.Core;
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
                var parametros = new DynamicParameters();
                parametros.Add("Id_Libro", reserva.Id_Libro);
                parametros.Add("Id_Usuario", reserva.Id_Usuario);
                parametros.Add("FechaInicio", reserva.FechaReserva);
                parametros.Add("FechaFin", reserva.FechaVencimiento);


                var resultado = context.QueryFirstOrDefault<int>(
            "ReservarLibro",
            parametros,
            commandType: CommandType.StoredProcedure
        );

                if (resultado == -1)
                    return BadRequest("No hay ejemplares disponibles.");

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
                int consecutivoUsuario = int.TryParse(HttpContext.User.FindFirst("id")?.Value, out var id) ? id : 0;
                var parametroValidar = new DynamicParameters();
                parametroValidar.Add("@Id_Usuario", consecutivoUsuario);
                var sanciones = context.ExecuteScalar<int>(
        "UsuarioTieneSancionActiva",
       parametroValidar,
        commandType: CommandType.StoredProcedure
    );

                if (sanciones > 0)
                {
                    return BadRequest("El usuario tiene una sanción activa y no puede realizar préstamos.");
                }
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
        [Route("CambiarEstadoCompletado")]
        public IActionResult CambiarEstadoCancelado(CambiarEstadoReservaRequestModel model)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Movimiento", model.Id_Movimiento);

                var resultado = context.Execute("CambiarEstadoCompletado", parametros);
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
                if (estado != null)
                {
                    parametros.Add("EstadoFiltro", estado);
                }

                var resultado = context.Query<ReservaResponseModel>(
                    "ObtenerReservasActivas",
                    parametros,
                    commandType: CommandType.StoredProcedure
                    );
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

        [HttpPost]
        [Route("RegistrarReservaAdmin")]
        public IActionResult RegistrarReservaAdmin(ReservaAdminRequestModel reserva)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var helper = new Helper();
                var sanciones = context.ExecuteScalar<int>(
        "UsuarioTieneSancionActiva",
        new { Id_Usuario = reserva.Id_Usuario },
        commandType: CommandType.StoredProcedure
    );

                if (sanciones > 0)
                {
                    return BadRequest("El usuario tiene una sanción activa y no puede realizar préstamos.");
                }
                var parametros = new DynamicParameters();

                parametros.Add("@Id_Ejemplar", reserva.Id_Ejemplar);
                parametros.Add("@Fecha", reserva.Fecha);
                parametros.Add("@Fecha_Vencimiento", reserva.Fecha_Vencimiento);
                parametros.Add("@Id_Usuario", reserva.Id_Usuario);

                var resultado = context.QueryFirstOrDefault<ReservaAdminResponseModel>("RegistrarReservaPorAdmin", parametros, commandType: CommandType.StoredProcedure);

                var ruta = Path.Combine(_environment.ContentRootPath, "PlantillasCorreo", "NotificacionReserva.html");
                var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                html = html.Replace("{{Usuario}}", reserva.Id_Usuario.ToString());
                html = html.Replace("{{Libro}}", resultado?.Titulo);
                html = html.Replace("{{FechaReserva}}", reserva.Fecha.ToString("F"));
                html = html.Replace("{{FechaVencimiento}}", reserva.Fecha_Vencimiento.ToString("F"));

                string? correoUsuario = resultado?.Correo;
                if (correoUsuario != null)
                {
                    helper.EnviarCorreo("Confirmación de reserva exitosa", html, correoUsuario);
                }

                return Ok(resultado);
            }

        }
    }


}









