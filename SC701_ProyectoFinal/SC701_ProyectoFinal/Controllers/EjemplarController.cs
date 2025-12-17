using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    public class EjemplarController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public EjemplarController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;            
        }


        public async Task<IActionResult> Index()
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Ejemplar/ObtenerEjemplares";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    var datos = await respuesta.Content.ReadFromJsonAsync<List<EjemplarModel>>();
                    return View(datos);
                }

                ViewBag.Mensaje = "No se encontraron ejemplares";
                return View(new List<EjemplarModel>());
            }
        }


        private async Task CargarLibros()
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Libro/ListarLibros";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    var datos = await respuesta.Content.ReadFromJsonAsync<List<LibroModel>>();
                    ViewBag.Libros = new SelectList(datos, "Id_Libro", "Titulo");
                    return;
                }

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
            if (!ModelState.IsValid)
            {
                await CargarLibros();
                return View(ejemplar);
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Ejemplar/RegistrarEjemplar";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var request = new EjemplarRequestModel
                {
                    CodigoEjemplar = ejemplar.CodigoEjemplar,
                    Id_Libro = ejemplar.Id_Libro,
                    Estado = ejemplar.Estado,
                    Ubicacion = ejemplar.Ubicacion,
                    Cantidad = ejemplar.Cantidad
                };

                var respuesta = await client.PostAsJsonAsync(urlApi, request);

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "No se pudo registrar el ejemplar.");
                await CargarLibros();
                return View(ejemplar);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Ejemplar/" + id;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.GetAsync(urlApi);

                if (!respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var ejemplar = await respuesta.Content.ReadFromJsonAsync<EjemplarModel>();

                if (ejemplar == null)
                    return NotFound();

                await CargarLibros();
                return View(ejemplar);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, EjemplarModel ejemplar)

        {
            if (!ModelState.IsValid)
            {
                await CargarLibros();
                return View(ejemplar);
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Ejemplar/" + ejemplar.Id_Ejemplar;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var request = new EjemplarRequestModel
                {
                    CodigoEjemplar = ejemplar.CodigoEjemplar,
                    Id_Libro = ejemplar.Id_Libro,
                    Estado = ejemplar.Estado,
                    Ubicacion = ejemplar.Ubicacion
                };

                var respuesta = await client.PutAsJsonAsync(urlApi, request);

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "No se pudo actualizar el ejemplar.");
                await CargarLibros();
                return View(ejemplar);
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Ejemplar/" + id;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.GetAsync(urlApi);

                if (!respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var ejemplar = await respuesta.Content.ReadFromJsonAsync<EjemplarModel>();

                if (ejemplar == null)
                    return NotFound();

                return View("Delete", ejemplar);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int Id_Ejemplar)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Ejemplar/" + Id_Ejemplar;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.DeleteAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                // cargar nuevamente el ejemplar para mostrarlo en la vista
                var getResponse = await client.GetAsync(urlApi);
                EjemplarModel ejemplar = null;

                if (getResponse.IsSuccessStatusCode)
                {
                    ejemplar = await getResponse.Content.ReadFromJsonAsync<EjemplarModel>();
                }

                ModelState.AddModelError("", "No se pudo eliminar el ejemplar.");
                return View("Delete", ejemplar);
            }
        }
    }
}