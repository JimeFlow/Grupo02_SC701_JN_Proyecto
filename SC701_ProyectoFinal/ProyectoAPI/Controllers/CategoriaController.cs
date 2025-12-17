using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;

namespace ProyectoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CategoriaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerCategorias")]
        public IActionResult ObtenerCategorias()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();


                var resultado = context.Query("ObtenerCategorias", parametros, commandType: CommandType.StoredProcedure);

                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("AgregarCategoria")]
        public IActionResult AgregarCategoria(CategoriaRequestModel categoria)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@Tipo", categoria.Tipo);
                

                var resultado = context.Execute("RegistrarCategoria", parametros, commandType: CommandType.StoredProcedure);

                return Ok(resultado);
            }
        }

        [HttpPut]
        [Route("EditarCategoria")]
        public IActionResult EditarCategoria(EditarCategoriaRequestModel categoria)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@Id_Categoria", categoria.Id_Categoria);
                parametros.Add("@Tipo", categoria.Tipo);


                var resultado = context.Execute("EditarCategoria", parametros, commandType: CommandType.StoredProcedure);

                return Ok(resultado);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarCategoria(int id)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Id_Categoria", id);

                var resultado = context.QueryFirst<int>(
                    "EliminarCategoria",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (resultado == 0)
                    return BadRequest("No se puede eliminar la categoría porque tiene libros asociados.");

                return Ok();
            }
        }

    }
}
