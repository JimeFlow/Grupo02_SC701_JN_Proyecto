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
        public IActionResult Crear(int idEjemplar, string comentario, int rating, int idLibro)
        {
            var idUsuario = HttpContext.Session.GetInt32("Id_Usuario");
            if (idUsuario == null)
            {
                TempData["ErrorComentario"] = "Debe iniciar sesión para comentar.";
                return RedirectToAction("Detalle", "Libro", new { id = idLibro });
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Comentarios/Crear";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var request = new
                {
                    Id_Usuario = idUsuario.Value,
                    Id_Ejemplar = idEjemplar,
                    Comentario = comentario,
                    Rating = rating
                };

                var response = client.PostAsJsonAsync(urlApi, request).Result;

                if (response.IsSuccessStatusCode)
                    TempData["MensajeComentario"] = "Comentario agregado correctamente.";
                else
                    TempData["ErrorComentario"] = "No se pudo agregar el comentario.";
            }

            return RedirectToAction("Detalle", "Libro", new { id = idLibro });
        }

    }

}