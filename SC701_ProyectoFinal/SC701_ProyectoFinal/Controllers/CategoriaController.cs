using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public CategoriaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _http = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Categoria/ObtenerCategorias";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<List<CategoriaModel>>().Result;
                    return View(datosApi);
                }

                ViewBag.Mensaje = "No hay categorias registrados";
                return View(new List<CategoriaModel>());
            }
        }

        [HttpPost]
        public IActionResult AgregarCategoria(CategoriaModel categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Categoria/AgregarCategoria";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PostAsJsonAsync(urlApi, categoria).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }

                ViewBag.Mensaje = "No se ha registrado la información" + respuesta;
                return RedirectToAction("Index", categoria);
            }
        }

        [HttpPost] 
        public IActionResult EditarCategoria(CategoriaModel categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Categoria/EditarCategoria";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, categoria).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }

                ViewBag.Mensaje = "No se ha actualizado la información" + respuesta;
                return RedirectToAction("Index",categoria);
            }
        }
    }
}
