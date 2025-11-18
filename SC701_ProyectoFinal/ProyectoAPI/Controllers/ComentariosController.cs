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
            try
            {
                // Lógica para crear un comentario en la base de datos
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BiblioSolaris")))
                {
                    SqlCommand command = new SqlCommand("AgregarComentario", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("Id_Usuario", comentario.UsuarioId);
                    command.Parameters.AddWithValue("Id_Libro", comentario.LibroId);
                    command.Parameters.AddWithValue("Comentario", comentario.ComentarioId);
                    command.Parameters.AddWithValue("Rating", comentario.Rating);
                    comentario.FechaCreacion = DateTime.Now;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                return Ok("Comentario y calificación registrados exitosamente.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al guardar tu comentario: {ex.Message}");
            }
        }

        [HttpGet("ListarPorLibro/{libroId}")]
        public IActionResult ListarComentarios(int libroId)
        {
            try
            {
                List<ComentarioModel> comentarios = new List<ComentarioModel>();
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BiblioSolaris")))
                {
                    SqlCommand command = new SqlCommand("ListarComentariosPorLibro", connection)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure
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

                            NombreUsuario = reader["NombreUsuario"] + " " + reader["ApellidoUsuario"],
                            Libro = reader["Titulo"].ToString(),
                            Comentario = reader["Comentario"].ToString(),
                            Rating = Convert.ToInt32(reader["Rating"]),
                            FechaCreacion = Convert.ToDateTime(reader["Fecha_Creacion"])
                        });
                    }
                    connection.Close();
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
