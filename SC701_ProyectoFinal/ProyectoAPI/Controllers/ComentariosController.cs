using System.Data;  
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComentariosController : Controller
    {
        private readonly IConfiguration _configuration;

        public ComentariosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Crear")]
        public IActionResult CrearComentario([FromBody] ComentarioModel comentario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", comentario.Id_Usuario);
                parametros.Add("@Id_Libro", comentario.Id_Libro);
                parametros.Add("@Comentario", comentario.Comentario);
                parametros.Add("@Rating", comentario.Rating);

                context.Execute("AgregarComentario", parametros, commandType: CommandType.StoredProcedure);
                return Ok(new { mensaje = "Comentario registrado exitosamente." });
            }
        }
        [HttpGet("ListarPorLibro/{libroId}")]
        public IActionResult ListarComentarios(int libroId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Libro", libroId);

                var comentarios = context.Query<ComentarioViewModel>(
                    "ListarComentariosPorLibro",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(comentarios);
            }
        }

    }
}
