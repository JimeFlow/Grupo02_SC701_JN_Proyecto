using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class ContactoController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public ContactoController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        // GET: Contacto
        public IActionResult Index()
        {

            if (TempData["DesdeEnvio"] == null)
            {
                TempData.Remove("MensajeExito");
                TempData.Remove("Error");
            }

            return View();
        }

        // POST: Enviar contacto
        [HttpPost]
        public IActionResult Enviar(ContactoViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            using var client = _http.CreateClient();
            var url = _configuration["Valores:UrlAPI"] + "Contacto/Crear";

            var response = client.PostAsJsonAsync(url, model).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["MensajeExito"] =
                    "Respuesta enviada correctamente. Pronto alguien se comunicará contigo.";
                TempData["DesdeEnvio"] = true; // 👈 bandera
                return RedirectToAction("Index");
            }

            ViewBag.Error = "No se pudo enviar el mensaje.";
            return View("Index", model);
        }

        // ADMIN – listar consultas
        public IActionResult Admin()
        {
            var rol = HttpContext.Session.GetInt32("Id_Rol");

            if (rol != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue
                ("Bearer", 
                HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] + "Contacto/Admin";
            var response = client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
                return View(new List<ContactoViewModel>());

            var datos = response.Content
                .ReadFromJsonAsync<List<ContactoViewModel>>()
                .Result;
            return View(datos);
        }

        [HttpPost]
        public IActionResult Responder(int idContacto, string respuesta)
        {
            using var client = _http.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] + "Contacto/Responder";

            var body = new
            {
                Id_Contacto = idContacto,
                Respuesta = respuesta
            };

            var response = client.PostAsJsonAsync(url, body).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["MensajeExito"] = "Respuesta enviada correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo enviar la respuesta.";
            }

            return RedirectToAction("Admin");
        }

        public IActionResult MisConsultas()
        {
            var rol = HttpContext.Session.GetInt32("Id_Rol");

            // Solo usuarios
            if (rol != 2)
                return RedirectToAction("Index", "Home");

            var correo = HttpContext.Session.GetString("Correo");

            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"]
                + $"Contacto/Usuario?correo={correo}";

            var response = client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
                return View(new List<ContactoViewModel>());

            var datos = response.Content
                .ReadFromJsonAsync<List<ContactoViewModel>>()
                .Result;

            return View(datos);
        }
    }

}
