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
        public IActionResult CrearComentario([FromBody] ComentarioRequestModel comentario)
        {
            using (var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Usuario", comentario.Id_Usuario);
                parametros.Add("@Id_Ejemplar", comentario.Id_Ejemplar); 
                parametros.Add("@Comentario", comentario.Comentario);
                parametros.Add("@Rating", comentario.Rating);

                connection.Execute(
                    "AgregarComentario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok("Comentario registrado correctamente");
            }
        }

        [HttpGet("Libro/{idLibro}")]
        public IActionResult ListarPorLibro(int idLibro)
        {
            using (var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Libro", idLibro);

                var comentarios = connection.Query<ComentarioResponseModel>(
                    "ListarComentariosPorLibro",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(comentarios);
            }
        }

    }

}

