using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class EjemplarController : Controller
    {

        private readonly HttpClient _httpClient;

        public EjemplarController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7028/api/");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("Ejemplar");

            if (!response.IsSuccessStatusCode)
                return View(new List<EjemplarModel>());

            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonConvert.DeserializeObject<List<EjemplarModel>>(json);

            return View(lista);
        }


        private async Task CargarLibros()
        {
            var response = await _httpClient.GetAsync("Libro");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var libros = JsonConvert.DeserializeObject<List<LibroModel>>(json);

                ViewBag.Libros = new SelectList(libros, "Id_Libro", "Titulo");
            }
            else
            {
                ViewBag.Libros = new SelectList(new List<LibroModel>(), "Id_Libro", "Titulo");
            }
        }

        public async Task<IActionResult> Create()
        {
            await CargarLibros();
            return View();
        }
       

        [HttpPost]
        public async Task<IActionResult> Create(EjemplarModel ejemplar)
        {
            Console.WriteLine("Entró Create Ejemplar POST");
            if (!ModelState.IsValid)
            {
                await CargarLibros();
                return View(ejemplar);
            }
            var request = new EjemplarRequestModel
            {
                CodigoEjemplar = ejemplar.CodigoEjemplar,
                Id_Libro = ejemplar.Id_Libro,
                Estado = ejemplar.Estado,
                Ubicacion = ejemplar.Ubicacion
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Ejemplar", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "No se pudo registrar el ejemplar");
            await CargarLibros();
            return View(ejemplar);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"Ejemplar/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await response.Content.ReadAsStringAsync();
            var ejemplar = JsonConvert.DeserializeObject<EjemplarModel>(json);

            if (ejemplar == null)
                return NotFound();
            
            await CargarLibros();
            return View(ejemplar);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, EjemplarModel ejemplar)

        { 

            if (!ModelState.IsValid)
            {
                await CargarLibros();
                return View(ejemplar);
            }

                var request = new EjemplarRequestModel
                {       
                    CodigoEjemplar = ejemplar.CodigoEjemplar,
                    Id_Libro = ejemplar.Id_Libro,
                    Estado = ejemplar.Estado,
                    Ubicacion = ejemplar.Ubicacion
                };
        
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"Ejemplar/{id}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", "⚠️ No se pudo actualizar el ejemplar.");
                await CargarLibros();
                return View(ejemplar);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"Ejemplar/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await response.Content.ReadAsStringAsync();
            var ejemplar = JsonConvert.DeserializeObject<EjemplarModel>(json);

            if (ejemplar == null)
                return NotFound();

            return View("Delete");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"Ejemplar/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var getResponse = await _httpClient.GetAsync($"Ejemplar/{id}");
            EjemplarModel ejemplar = null;

            if (getResponse.IsSuccessStatusCode)

            {
                var getJson = await getResponse.Content.ReadAsStringAsync();
                ejemplar = JsonConvert.DeserializeObject<EjemplarModel>(getJson);
            }

            ModelState.AddModelError("", "⚠️ No se pudo eliminar el ejemplar.");
            return View("Delete", ejemplar);
        }
    }
}