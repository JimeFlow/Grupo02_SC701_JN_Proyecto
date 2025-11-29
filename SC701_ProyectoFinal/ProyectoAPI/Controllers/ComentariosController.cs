using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;  
using System.Data;  

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
            try
            {

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    SqlCommand command = new SqlCommand("AgregarComentario", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id_Usuario", comentario.Id_Usuario);
                    command.Parameters.AddWithValue("@Id_Libro", comentario.Id_Libro);
                    command.Parameters.AddWithValue("@Comentario", comentario.Comentario);
                    command.Parameters.AddWithValue("@Rating", comentario.Rating);


                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return Ok(new { mensaje = "Comentario registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("ListarPorLibro/{libroId}")]
        public IActionResult ListarComentarios(int libroId)
        {
            try
            {
                List<ComentarioViewModel> comentarios = new List<ComentarioViewModel>();
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    SqlCommand command = new SqlCommand("ListarComentariosPorLibro", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@Id_Libro", libroId);
                    connection.Open();

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        comentarios.Add(new ComentarioViewModel
                        {
                            ComentarioId = Convert.ToInt32(reader["Id_Comentario"]),
                            UsuarioId = Convert.ToInt32(reader["Id_Usuario"]),
                            LibroId = Convert.ToInt32(reader["Id_Libro"]),

                            NombreUsuario = $"{reader["NombreUsuario"]} {reader["ApellidoUsuario"]}",
                            Libro = reader["Titulo"].ToString(),
                            Comentario = reader["Comentario"].ToString(),
                            Rating = Convert.ToInt32(reader["Rating"]),
                            FechaCreacion = Convert.ToDateTime(reader["Fecha_Creacion"])
                        });
                    }
                }
                return Ok(comentarios);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener los comentarios: {ex.Message}");
            }
        }
    }
}
