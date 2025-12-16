using System.Data;
using Dapper;
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
        [Route("ObtenerEjemplares")]
        public IActionResult ObtenerEjemplares()
        {

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var resultado = context.Query<EjemplarModel>("ObtenerEjemplares");
                return Ok(resultado);
            }
        }



        // GET: api/ejemplar
        [HttpGet("{id}")]
        public IActionResult ObtenerEjemplarPorId(int id)
        {

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id", id);

                var ejemplar = context.QueryFirstOrDefault<EjemplarModel>(
                    "ObtenerEjemplarPorId",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (ejemplar == null)
                    return NotFound();

                return Ok(ejemplar);
            }
        }
            
          

        // POST: api/ejemplar
        [HttpPost]
        [Route("RegistrarEjemplar")]
        public IActionResult RegistrarEjemplar([FromBody] EjemplarRequestModel ejemplar)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@CodigoEjemplar", ejemplar.CodigoEjemplar);
                parametros.Add("@Id_Libro", ejemplar.Id_Libro);
                parametros.Add("@Estado", ejemplar.Estado);
                parametros.Add("@Ubicacion", ejemplar.Ubicacion);

                context.Execute("RegistrarEjemplar", parametros);
            }

            return Ok(new { mensaje = "Ejemplar registrado correctamente." });
        }

        // PUT: api/ejemplar/5
        [HttpPut("{id}")]
        public IActionResult ActualizarEjemplar(int id, [FromBody] EjemplarRequestModel ejemplar)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id", id);
                parametros.Add("@CodigoEjemplar", ejemplar.CodigoEjemplar);
                parametros.Add("@Id_Libro", ejemplar.Id_Libro);
                parametros.Add("@Estado", ejemplar.Estado);
                parametros.Add("@Ubicacion", ejemplar.Ubicacion);

                context.Execute("ActualizarEjemplar", parametros);
            }

            return Ok(new { mensaje = "Ejemplar actualizado correctamente." });
        }

        // DELETE: api/ejemplar/5
        [HttpDelete("{id}")]
        public IActionResult EliminarEjemplar(int id)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id", id);

                context.Execute("EliminarEjemplar", parametros);
            }

            return Ok(new { mensaje = "Ejemplar eliminado correctamente." });
        }

        [HttpGet]
        [Route("ObtenerEjemplaresDisponiblesAdmin")]
        public IActionResult ObtenerEjemplaresDisponiblesAdmin()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var resultado = context.Query<EjemplarModel>("ObtenerEjemplaresDisponiblesAdmin");
                return Ok(resultado);
            }
        }
    }
}