using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    public class HomeController : Controller
    {

        private static InformacionViewModel _informacion = new InformacionViewModel
        {
            Titulo = "Biblioteca Digital",
            Subtitulo = "Plataforma para la gestión y consulta de recursos bibliográficos",
            Texto = "Nuestra biblioteca es un espacio diseñado para apoyar el aprendizaje...",
            Mision = "Brindar acceso organizado y confiable...",
            Vision = "Ser una biblioteca moderna y accesible...",
            Servicios = @"Consulta de libros
Reservas y préstamos
Gestión de usuarios
Comentarios y calificaciones
Preguntas frecuentes",
            Horario = "L–V 8:00 a.m. – 6:00 p.m. y Sábados 8:00 a.m. – 12:00 m.m."
        };

        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory http, IConfiguration configuration)
        {
            _logger = logger;
            _http = http;
            _configuration = configuration;
        }
        [Seguridad]
        public IActionResult Index() 
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Libro/ListarLibros";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var libros = respuesta.Content.ReadFromJsonAsync<List<LibroModel>>().Result;
                    return View(libros);
                }

                ViewBag.Mensaje = "No hay libros registrados";
                return View(new List<LibroModel>());
            }
        }

       

        public IActionResult Terminos()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Informacion()
        {
            return View(_informacion);

        }

        public IActionResult EditarInformacion()
        {
            var rol = HttpContext.Session.GetInt32("Id_Rol");

            if (rol != 1)
                return RedirectToAction("Index");

            return View(_informacion);
        }

        [HttpPost]
        public IActionResult EditarInformacion(InformacionViewModel model)
        {
            var rol = HttpContext.Session.GetInt32("Id_Rol");
            if (rol != 1)
                return RedirectToAction("Index");

            _informacion = model;

            TempData["MensajeExito"] = "Información actualizada correctamente.";
            TempData["DesdeEdicion"] = true;

            TempData["MensajeExito"] = "Información actualizada correctamente.";

            return View("Informacion", _informacion);
        }

       
    }
}
