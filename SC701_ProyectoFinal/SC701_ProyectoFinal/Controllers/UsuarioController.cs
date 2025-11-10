using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    [Seguridad]
    public class UsuarioController : Controller
    {

        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public UsuarioController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _http = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ListaUsuarios";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<List<UsuarioModel>>().Result;
                    return View(datosApi);
                }

                ViewBag.Mensaje = "No hay productos registrados";
                return View(new List<UsuarioModel>());
            }
        }

        [HttpGet]
        public IActionResult RegistrarUsuarioAdmin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarUsuarioAdmin(UsuarioModel usuario)
        {
            using (var context = _http.CreateClient())
            {                
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/RegistrarUsuario";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PostAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("ConsultarProductos", "Producto");
                    }
                }

                ViewBag.Mensaje = "No se ha registrado la información" + respuesta;
                return View();
            }
        }


    }
}
