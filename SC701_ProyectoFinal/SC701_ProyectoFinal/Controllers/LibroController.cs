using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class LibroController : Controller
    {
        private readonly HttpClient _httpClient;

        public LibroController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7028/api/"); 
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("Libro");
            if (!response.IsSuccessStatusCode)
                return View(new List<LibroModel>());

            var json = await response.Content.ReadAsStringAsync();
            var libros = JsonConvert.DeserializeObject<List<LibroModel>>(json);
            return View(libros);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(LibroModel libro)
        {
            Console.WriteLine("Entró Create Libro POST");
            var request = new LibroRequestModel
            {
                ISBN = libro.ISBN,
                Estado_Libro = libro.Estado_Libro,
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                Anio = libro.Anio,
                Imagen_URL = libro.Imagen_URL,
                Id_Estado = libro.Id_Estado
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Libro/RegistrarLibro", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));
            else
                ModelState.AddModelError("", "No se pudo registrar el libro");

            return View(libro);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync("Libro");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await response.Content.ReadAsStringAsync();
            var libros = JsonConvert.DeserializeObject<List<LibroModel>>(json);
            var libro = libros.FirstOrDefault(x => x.Id_Libro == id);

            if (libro == null)
                return RedirectToAction(nameof(Index));

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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"Libro/EliminarLibro/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var listResponse = await _httpClient.GetAsync("Libro");
            LibroModel libro = null;

            if (listResponse.IsSuccessStatusCode)
            {
                var jsonList = await listResponse.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<List<LibroModel>>(jsonList);

                libro = lista.FirstOrDefault(x => x.Id_Libro == id);
            }

            ModelState.AddModelError("", "No se pudo eliminar el libro.");
            return View("Delete",libro);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync("Libro");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await response.Content.ReadAsStringAsync();
            var libros = JsonConvert.DeserializeObject<List<LibroModel>>(json);
            var libro = libros.FirstOrDefault(l => l.Id_Libro == id);

            if (libro == null)
                return NotFound();

            return View(libro);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, LibroModel libro)
        {
            var json = JsonConvert.SerializeObject(libro);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"Libro/ActualizarLibro/{id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "No se pudo actualizar el libro");
            return View(libro);
        }
    }
}
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
