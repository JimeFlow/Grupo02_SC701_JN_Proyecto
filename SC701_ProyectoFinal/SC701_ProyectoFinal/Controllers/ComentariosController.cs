using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace SC701_ProyectoFinal.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ComentariosController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // POST: Agregar comentario
        [HttpPost]
        public async Task<IActionResult> Crear(
      int idLibro,
      int idEjemplar,
      string comentario,
      int rating)
        {
            var body = new
            {
                Id_Usuario = HttpContext.Session.GetInt32("Id_Usuario"),
                Id_Ejemplar = idEjemplar,
                Comentario = comentario,
                Rating = rating
            };

            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            var response = await client.PostAsJsonAsync("Comentarios/Crear", body);

            if (response.IsSuccessStatusCode)
            {
                TempData["MensajeComentario"] = "Comentario agregado correctamente";
            }
            else
            {
                TempData["ErrorComentario"] = "No se pudo agregar el comentario";
            }

            return RedirectToAction("Detalle", "Libro", new { id = idLibro });
        }

    }

}