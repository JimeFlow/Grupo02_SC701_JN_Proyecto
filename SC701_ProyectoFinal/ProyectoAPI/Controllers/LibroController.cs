using System.Data;
using System.Net.Http;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibroController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LibroController(IConfiguration configuration)
        {
            _configuration = configuration;
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

        // Registra nuevo libro
        [HttpPost]
        [Route("RegistrarLibro")]
        public IActionResult RegistrarLibro([FromBody] LibroRequestModel libro)
        {
            
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@ISBN", libro.ISBN);
                parametros.Add("@Estado_Libro", libro.Estado_Libro);
                parametros.Add("@Descripcion", libro.Descripcion);
                parametros.Add("@Titulo", libro.Titulo);
                parametros.Add("@Autor", libro.Autor);
                parametros.Add("@Anio", libro.Anio);
                parametros.Add("@Imagen_URL", libro.Imagen_URL);
                parametros.Add("@Id_Estado", libro.Id_Estado);

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
                var parametros = new DynamicParameters();

                parametros.Add("@Id_Libro", id);
                parametros.Add("@ISBN", libro.ISBN);
                parametros.Add("@Estado_Libro", libro.Estado_Libro);
                parametros.Add("@Descripcion", libro.Descripcion);
                parametros.Add("@Titulo", libro.Titulo);
                parametros.Add("@Autor", libro.Autor);
                parametros.Add("@Anio", libro.Anio);
                parametros.Add("@Imagen_URL", libro.Imagen_URL);
                parametros.Add("@Id_Estado", libro.Id_Estado);

                var resultado = context.Execute("ActualizarLibro", parametros, commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Libro actualizado exitosamente" });
            }
        }

        // Elimina libro por ID
        [HttpDelete]
        [Route("EliminarLibro/{id}")]
        public IActionResult EliminarLibro(int id)
        {
            
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Libro", id);

                var res = context.Execute("EliminarLibro", parametros, commandType: CommandType.StoredProcedure);

                if (res > 0)
                    return Ok(new { mensaje = "Libro eliminado correctamente" });

                return NotFound(new { mensaje = "No se encontró el libro con ese ID" });
            }
        }

        // GET: api/Libro/5
        [HttpGet("{id}")]
        public IActionResult ObtenerLibro(int id)
        {
            
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Libro", id);

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
        public IActionResult ReservarLibro([FromBody] ReservaRequestModel request)
        {
            
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@Id_Libro", request.Id_Libro);
                parametros.Add("@Tipo", request.Tipo);
                parametros.Add("@Fecha", request.Fecha_Reserva);
                parametros.Add("@Fecha_Vencimiento", request.Fecha_Vencimiento);
                parametros.Add("@Estado", request.Id_Estado);
                parametros.Add("@Id_Usuario", request.Id_Usuario);

                var result = context.QueryFirstOrDefault<int>("ReservarLibro", parametros, commandType: CommandType.StoredProcedure);

                if (result == -1)
                    return BadRequest(new { mensaje = "El libro no está disponible para reservar." });

                return Ok(new { mensaje = "Libro reservado correctamente." });
            }
        }


    }
}