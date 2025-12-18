using System.Data;
using System.Net.Http;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using Utils;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibroController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public LibroController(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // Lista todos los libros
        [HttpGet]
        [Route("ListarLibros")]
        public IActionResult ObtenerLibros()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                var resultado = context.Query<LibroResponseModel>("ListarLibros", parametros, commandType: CommandType.StoredProcedure);
                return Ok(resultado);
            }

        }

        [HttpGet]
        [Route("ListarLibrosCliente")]
        public IActionResult ObtenerLibrosCliente()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                var resultado = context.Query<LibroResponseModel>("ObtenerListaLibrosCliente", parametros, commandType: CommandType.StoredProcedure);
                return Ok(resultado);
            }
        }

        // Registra nuevo libro
        [HttpPost]
        [Route("RegistrarLibro")]
        public IActionResult RegistrarLibro([FromBody] LibroRequestModel libro)
        {

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@ISBN", libro.ISBN);
                parametros.Add("@Descripcion", libro.Descripcion);
                parametros.Add("@Id_Categoria", libro.Id_Categoria);
                parametros.Add("@Titulo", libro.Titulo);
                parametros.Add("@Autor", libro.Autor);
                parametros.Add("@Anio", libro.Anio);
                parametros.Add("@Imagen_URL", libro.Imagen_URL);

                var resultado = context.Execute("RegistrarLibro", parametros, commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Libro registrado exitosamente" });
            }
        }

        // Actualiza libro existente
        [HttpPut]
        [Route("ActualizarLibro/{id}")]
        public IActionResult ActualizarLibro(int id, [FromBody] LibroRequestModel libro)
        {

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                int consecutivoUsuario = int.TryParse(HttpContext.User.FindFirst("id")?.Value, out var idU) ? idU
             : 0;
                var parametros = new DynamicParameters();

                parametros.Add("@Id_Libro", id);
                parametros.Add("@ISBN", libro.ISBN);
                parametros.Add("@Descripcion", libro.Descripcion);
                parametros.Add("@Titulo", libro.Titulo);
                parametros.Add("@Autor", libro.Autor);
                parametros.Add("@Anio", libro.Anio);
                parametros.Add("@Imagen_URL", libro.Imagen_URL);
                parametros.Add("@Id_Usuario", consecutivoUsuario);
                parametros.Add("@Id_Categoria", libro.Id_Categoria);

                var resultado = context.Execute("ActualizarLibro", parametros, commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Libro actualizado exitosamente" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarLibro(int id)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Libro", id);

                var resultado = context.QueryFirst<int>(
                    "EliminarLibro",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (resultado == 0)
                    return BadRequest("No se puede eliminar el libro porque tiene ejemplares registrados.");

                return Ok();
            }
        }

        // GET: api/Libro/5
        [HttpGet("{id}")]
        public IActionResult ObtenerLibro(int id)
        {

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id", id);

                var libro = context.QueryFirstOrDefault<LibroResponseModel>(
                    "ObtenerLibroPorId",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (libro == null)
                    return NotFound(new { mensaje = "No se encontró el libro" });

                return Ok(libro);
            }
        }

        [HttpPost]
        [Route("ReservarLibro")]
        public IActionResult ReservarLibro([FromBody] ReservaLibroModel request)
        {

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                string nombreUsuario = HttpContext.User.FindFirst("nombre")?.Value ?? "";
                var helper = new Helper();
                var sanciones = context.ExecuteScalar<int>(
        "UsuarioTieneSancionActiva",
        new { Id_Usuario = request.Id_Usuario },
        commandType: CommandType.StoredProcedure
    );

                if (sanciones > 0)
                {
                    return BadRequest("El usuario tiene una sanción activa y no puede realizar préstamos.");
                }

                var parametros = new DynamicParameters();

                parametros.Add("@Id_Libro", request.Id_Libro);
                parametros.Add("@Fecha", request.Fecha);
                parametros.Add("@Fecha_Vencimiento", request.Fecha_Vencimiento);
                parametros.Add("@Id_Usuario", request.Id_Usuario);

                var result = context.QueryFirstOrDefault<ReservaResponseModel>("ReservarLibro", parametros, commandType: CommandType.StoredProcedure);

                if (result.Resultado == -1)
                    return BadRequest(new { mensaje = "El libro no está disponible para reservar." });

                var ruta = Path.Combine(_environment.ContentRootPath, "PlantillasCorreo", "NotificacionReserva.html");
                var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                html = html.Replace("{{Usuario}}", nombreUsuario);
                html = html.Replace("{{Libro}}", result.Titulo); //nombre
                html = html.Replace("{{FechaReserva}}", request.Fecha.ToString("F"));
                html = html.Replace("{{FechaVencimiento}}", request.Fecha_Vencimiento.ToString("F"));

                string? correoUsuario = HttpContext.User.FindFirst("correo")?.Value;
                if( correoUsuario != null)
                {
                    helper.EnviarCorreo("Confirmación de reserva exitosa", html, correoUsuario);
                }

                return Ok(new { mensaje = "Libro reservado correctamente." });
            }
        }


    }
}