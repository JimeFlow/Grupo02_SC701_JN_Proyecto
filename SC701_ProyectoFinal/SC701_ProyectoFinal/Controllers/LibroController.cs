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
            var json = JsonConvert.SerializeObject(libro);
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

            return View(libro);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"Libro/EliminarLibro/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "No se pudo eliminar el libro.");
            return View();
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