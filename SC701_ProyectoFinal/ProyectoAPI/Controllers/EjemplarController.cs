using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EjemplarController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EjemplarController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ObtenerEjemplares()
        {

            List<EjemplarModel> lista = new();

            using (SqlConnection con = new(_configuration.GetConnectionString("BDConnection")))
            {
                con.Open();
                string query = @"SELECT E.Id_Ejemplar, E.CodigoEjemplar, E.Estado, E.Ubicacion, 
                                 E.Fecha_Registro, E.Id_Libro, L.Titulo
                                 FROM Ejemplar E 
                                 INNER JOIN Libro L ON E.Id_Libro = L.Id_Libro";

                SqlCommand cmd = new(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new EjemplarModel
                    {
                        Id_Ejemplar = Convert.ToInt32(reader["Id_Ejemplar"]),
                        CodigoEjemplar = reader["CodigoEjemplar"].ToString(),
                        Estado = reader["Estado"].ToString(),
                        Ubicacion = reader["Ubicacion"].ToString(),
                        Fecha_Registro = Convert.ToDateTime(reader["Fecha_Registro"]),

                        Id_Libro = Convert.ToInt32(reader["Id_Libro"]),
                        TituloLibro = reader["Titulo"].ToString()
                    });
                }
            }
            
                return Ok(lista);
        }



        // GET: api/ejemplar
        [HttpGet("{id}")]
        public IActionResult ObtenerEjemplarPorId(int id)
        {

            EjemplarModel ejemplar = null;

            using (SqlConnection con = new(_configuration.GetConnectionString("BDConnection")))
            {
                con.Open();
                string query = @"SELECT E.Id_Ejemplar, E.CodigoEjemplar, E.Estado, E.Ubicacion, 
                                 E.Fecha_Registro, E.Id_Libro, L.Titulo
                                 FROM Ejemplar E 
                                 INNER JOIN Libro L ON E.Id_Libro = L.Id_Libro
                                 WHERE E.Id_Ejemplar = @Id";

                SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ejemplar = new EjemplarModel
                    {
                        Id_Ejemplar = Convert.ToInt32(reader["Id_Ejemplar"]),
                        CodigoEjemplar = reader["CodigoEjemplar"].ToString(),
                        Estado = reader["Estado"].ToString(),
                        Ubicacion = reader["Ubicacion"].ToString(),
                        Fecha_Registro = Convert.ToDateTime(reader["Fecha_Registro"]),

                        Id_Libro = Convert.ToInt32(reader["Id_Libro"]),
                        TituloLibro = reader["Titulo"].ToString()
                    };
                }
            }

            if (ejemplar == null)
                return NotFound();

                return Ok(ejemplar);
        }
            
          

        // POST: api/ejemplar
        [HttpPost]
        public IActionResult RegistrarEjemplar([FromBody] EjemplarRequestModel ejemplar)
        {
            using (SqlConnection con = new(_configuration.GetConnectionString("BDConnection")))
            {
                con.Open();
                string query = @"INSERT INTO Ejemplar (CodigoEjemplar, Id_Libro, Estado, Ubicacion)
                                 VALUES (@CodigoEjemplar, @Id_Libro, @Estado, @Ubicacion)";

                SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@CodigoEjemplar", ejemplar.CodigoEjemplar);
                cmd.Parameters.AddWithValue("@Id_Libro", ejemplar.Id_Libro);
                cmd.Parameters.AddWithValue("@Estado", ejemplar.Estado);
                cmd.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Ejemplar registrado correctamente." });
        }

        // PUT: api/ejemplar/5
        [HttpPut("{id}")]
        public IActionResult ActualizarEjemplar(int id, [FromBody] EjemplarRequestModel ejemplar)
        {
            using (SqlConnection con = new(_configuration.GetConnectionString("BDConnection")))
            {
                con.Open();
                string query = @"UPDATE Ejemplar SET 
                                CodigoEjemplar=@CodigoEjemplar,
                                Estado=@Estado,
                                Ubicacion=@Ubicacion,
                                Id_Libro=@Id_Libro
                                WHERE Id_Ejemplar=@Id";

                SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CodigoEjemplar", ejemplar.CodigoEjemplar);
                cmd.Parameters.AddWithValue("@Id_Libro", ejemplar.Id_Libro);
                cmd.Parameters.AddWithValue("@Estado", ejemplar.Estado);
                cmd.Parameters.AddWithValue("@Ubicacion", ejemplar.Ubicacion ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Ejemplar actualizado correctamente." });
        }

        // DELETE: api/ejemplar/5
        [HttpDelete("{id}")]
        public IActionResult EliminarEjemplar(int id)
        {
            using (SqlConnection con = new(_configuration.GetConnectionString("BDConnection")))
            {
                con.Open();
                string query = "DELETE FROM Ejemplar WHERE Id_Ejemplar = @Id";
                SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Ejemplar eliminado correctamente." });
        }
    }
}