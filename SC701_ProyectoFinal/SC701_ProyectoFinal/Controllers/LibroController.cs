using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class LibroController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LibroController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> DetalleLibro(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            // Llamar a la API para obtener los detalles del libro
            var response = await client.GetAsync($"/api/Libro/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "No se puede cargar el libro.";
                return RedirectToAction("Index", "Home");   
            }
            
            var libro = await response.Content.ReadFromJsonAsync<LibroViewModel>();

            // Llamar a la API para obtener los comentarios del libro
            var comentariosResponse = await client.GetAsync($"/api/Comentarios/Libro/{id}");
            var comentarios = new List<ComentarioViewModel>();

            if (comentariosResponse.IsSuccessStatusCode)
            {
                comentarios = await comentariosResponse.Content.ReadFromJsonAsync<List<ComentarioViewModel>>();
            }

            libro.Comentarios = comentarios;
            return View(libro);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarComentario(ComentarioModel comentario)
        {
            var UsuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (UsuarioId == null)
            {
                return RedirectToAction("DetallesLibro", new { id = comentario.LibroId });
            }

            comentario.UsuarioId = UsuarioId.Value;

            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.PostAsJsonAsync("/api/Comentarios/Crear", comentario);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Comentario enviado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al enviar el comentario.";
            }

            return RedirectToAction("DetallesLibro", new { id = comentario.LibroId });
        }
    }
}
