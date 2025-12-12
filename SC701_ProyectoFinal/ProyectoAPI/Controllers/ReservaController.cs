using System.Data;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using ProyectoAPI.Services;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : Controller
    {
        private readonly IConfiguration _configuration;

        public ReservaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Crear")]
        public IActionResult CrearReserva([FromBody] ReservaRequestModel reserva)
        {
            // 1. Crear la reserva (Dapper + SP)
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", reserva.Id_Usuario);
                parametros.Add("@Id_Libro", reserva.Id_Libro);
                parametros.Add("@Fecha_Reserva", reserva.Fecha_Reserva);
                parametros.Add("@Fecha_Vencimiento", reserva.Fecha_Vencimiento);
                parametros.Add("@Id_Estado", reserva.Id_Estado);

                context.Execute(
                    "CrearReserva",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );
            }

            // 2. Obtener correo del usuario (SP + Dapper)
            string? correoUsuario;
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                correoUsuario = context.QueryFirstOrDefault<string>(
                    "ObtenerCorreoUsuario",
                    new { Id_Usuario = reserva.Id_Usuario },
                    commandType: CommandType.StoredProcedure
                );
            }

            if (string.IsNullOrEmpty(correoUsuario))
                return BadRequest("No se pudo obtener el correo del usuario.");

            // 3. Preparar correo (esto está perfecto)
            var ruta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "PlantillasCorreo",
                "NotificacionReserva.html"
            );

            string html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);
            html = html.Replace("{{Usuario}}", "Nombre");
            html = html.Replace("{{Libro}}", "Titulo");
            html = html.Replace("{{FechaReserva}}", reserva.Fecha_Reserva.ToString("dd/MM/yyyy"));
            html = html.Replace("{{FechaVencimiento}}", reserva.Fecha_Vencimiento.ToShortDateString());

            // (envío comentado está bien para entrega)
            // _correoService.EnviarCorreo(...)

            return Ok(new { mensaje = "¡Reserva creada exitosamente!" });
        }

    }

}