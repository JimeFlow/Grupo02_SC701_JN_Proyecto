using Microsoft.AspNetCore.Mvc;
using ProyectoAPI.Models;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Text;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibroController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LibroController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Lista todos los libros
        [HttpGet]
        public IActionResult ObtenerLibros()
        {
            List<LibroResponseModel> lista = new List<LibroResponseModel>();

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
            {
                conexion.Open();
                string query = "SELECT Id_Libro, ISBN, Estado_Libro, Titulo, Autor, Anio, Imagen_URL FROM Libro";
                SqlCommand cmd = new SqlCommand(query, conexion);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new LibroResponseModel
                    {
                        Id_Libro = Convert.ToInt32(reader["Id_Libro"]),
                        ISBN = reader["ISBN"].ToString(),
                        Estado_Libro = reader["Estado_Libro"].ToString(),
                        Titulo = reader["Titulo"].ToString(),
                        Autor = reader["Autor"].ToString(),
                        Anio = Convert.ToInt16(reader["Anio"]),
                        Imagen_URL = reader["Imagen_URL"].ToString()
                    });
                }
            }

            return Ok(lista);
        }

        // Registra nuevo libro
        [HttpPost]
        [Route("RegistrarLibro")]
        public IActionResult RegistrarLibro([FromBody] LibroRequestModel libro)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    conexion.Open();

                    string query = @"INSERT INTO Libro (ISBN, Estado_Libro, Titulo, Autor, Anio, Imagen_URL, Id_Estado)
                                     VALUES (@ISBN, @Estado_Libro, @Titulo, @Autor, @Anio, @Imagen_URL, @Id_Estado)";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
                    cmd.Parameters.AddWithValue("@Estado_Libro", libro.Estado_Libro);
                    cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
                    cmd.Parameters.AddWithValue("@Autor", libro.Autor);
                    cmd.Parameters.AddWithValue("@Anio", libro.Anio);
                    cmd.Parameters.AddWithValue("@Imagen_URL", libro.Imagen_URL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Estado", libro.Id_Estado);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        return Ok(new { mensaje = "Libro registrado exitosamente." });
                    else
                        return BadRequest(new { mensaje = "No se pudo registrar el libro." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor", error = ex.Message });
            }
        }

        // Actualiza libro existente
        [HttpPut]
        [Route("ActualizarLibro/{id}")]
        public IActionResult ActualizarLibro(int id, [FromBody] LibroRequestModel libro)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    conexion.Open();

                    string query = @"UPDATE Libro
                                     SET ISBN = @ISBN,
                                         Estado_Libro = @Estado_Libro,
                                         Titulo = @Titulo,
                                         Autor = @Autor,
                                         Anio = @Anio,
                                         Imagen_URL = @Imagen_URL,
                                         Id_Estado = @Id_Estado
                                     WHERE Id_Libro = @Id_Libro";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Id_Libro", id);
                    cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
                    cmd.Parameters.AddWithValue("@Estado_Libro", libro.Estado_Libro);
                    cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
                    cmd.Parameters.AddWithValue("@Autor", libro.Autor);
                    cmd.Parameters.AddWithValue("@Anio", libro.Anio);
                    cmd.Parameters.AddWithValue("@Imagen_URL", libro.Imagen_URL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Estado", libro.Id_Estado);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        return Ok(new { mensaje = "Libro actualizado exitosamente." });
                    else
                        return NotFound(new { mensaje = "No se encontró el libro con el ID especificado." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor", error = ex.Message });
            }
        }

        // Elimina libro por ID
        [HttpDelete]
        [Route("EliminarLibro/{id}")]
        public IActionResult EliminarLibro(int id)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
                {
                    conexion.Open();
                    string query = "DELETE FROM Libro WHERE Id_Libro = @Id";
                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        return Ok(new { mensaje = "Libro eliminado correctamente." });
                    else
                        return NotFound(new { mensaje = "No se encontró el libro con el ID especificado." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor", error = ex.Message });
            }
        }

        // GET: api/Libro/5
        [HttpGet("{id}")]
        public IActionResult ObtenerLibro(int id)
        {
            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
            {
                conexion.Open();
                string query = "SELECT Id_Libro, ISBN, Estado_Libro, Titulo, Autor, Anio, Imagen_URL, Id_Estado FROM Libro WHERE Id_Libro = @Id";
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var libro = new LibroResponseModel
                    {
                        Id_Libro = Convert.ToInt32(reader["Id_Libro"]),
                        ISBN = reader["ISBN"].ToString(),
                        Estado_Libro = reader["Estado_Libro"].ToString(),
                        Titulo = reader["Titulo"].ToString(),
                        Autor = reader["Autor"].ToString(),
                        Anio = Convert.ToInt16(reader["Anio"]),
                        Imagen_URL = reader["Imagen_URL"].ToString(),
                        Id_Estado = Convert.ToInt32(reader["Id_Estado"])
                    };
                    return Ok(libro);
                }
            }

            return NotFound(new { mensaje = "No se encontró el libro con el ID especificado." });
        }

        [HttpPost]
        [Route("ReservarLibro")]
        public IActionResult ReservarLibro([FromBody] ReservaRequestModel request)
        {
            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("BDConnection")))
            {
                conexion.Open();

                string check = "SELECT Estado_Libro FROM Libro WHERE Id_Libro = @Id_Libro";
                SqlCommand cmdCheck = new SqlCommand(check, conexion);
                cmdCheck.Parameters.AddWithValue("@Id_Libro", request.Id_Libro);
                var estadoActual = cmdCheck.ExecuteScalar()?.ToString();

                if (estadoActual != "Disponible")
                    return BadRequest(new { mensaje = "El libro no está disponible para reservar." });

                string updateLibro = "UPDATE Libro SET Estado_Libro = 'Reservado', Id_Estado = 2 WHERE Id_Libro = @Id_Libro";
                SqlCommand cmdUpdate = new SqlCommand(updateLibro, conexion);
                cmdUpdate.Parameters.AddWithValue("@Id_Libro", request.Id_Libro);
                cmdUpdate.ExecuteNonQuery();

                string insertMovimiento = @"INSERT INTO Movimiento 
                                   (Tipo, Fecha, Fecha_Vencimiento, Estado, Id_Usuario, Id_Libro) 
                                   VALUES (@Tipo, @Fecha, @Fecha_Vencimiento, @Estado, @Id_Usuario, @Id_Libro)";
                SqlCommand cmdInsert = new SqlCommand(insertMovimiento, conexion);
                cmdInsert.Parameters.AddWithValue("@Tipo", request.Tipo);
                cmdInsert.Parameters.AddWithValue("@Fecha", request.Fecha);
                cmdInsert.Parameters.AddWithValue("@Fecha_Vencimiento", request.Fecha_Vencimiento);
                cmdInsert.Parameters.AddWithValue("@Estado", request.Estado);
                cmdInsert.Parameters.AddWithValue("@Id_Usuario", request.Id_Usuario);
                cmdInsert.Parameters.AddWithValue("@Id_Libro", request.Id_Libro);
                cmdInsert.ExecuteNonQuery();

                return Ok(new { mensaje = "Libro reservado y movimiento registrado" });
            }
        }


    }
}