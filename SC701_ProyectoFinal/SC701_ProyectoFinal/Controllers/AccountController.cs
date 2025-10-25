using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;

using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        #region Iniciar Sesión

        [HttpGet]
        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public IActionResult Login(UsuarioModel usuario)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Account/IniciarSesion";
                var respuesta = context.PostAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                    if (datosApi != null)
                    {
                        HttpContext.Session.SetString("NombreUsuario", datosApi.Nombre);
                        HttpContext.Session.SetString("NombreRol", datosApi.Tipo_Rol);
                        return RedirectToAction("Index", "Home");
                    }
                }
                var errorMessage = respuesta.Content.ReadAsStringAsync().Result;
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(errorMessage);
                if(errorObj != null && errorObj.ContainsKey("mensaje"))
                {
                    ViewBag.Mensaje = errorObj["mensaje"];
                }
                else
                {
                    ViewBag.Mensaje = "Error desconocido en el proceso...";
                }
                return View();

            }
        }

        #endregion

        #region Registro

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UsuarioModel usuario)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Account/Registrarse";
                var respuesta = context.PostAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("Login");
                    }
                }
                var errorMessage = respuesta.Content.ReadAsStringAsync().Result;
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(errorMessage);
                if (errorObj != null && errorObj.ContainsKey("mensaje"))
                {
                    ViewBag.Mensaje = errorObj["mensaje"];
                }
                else
                {
                    ViewBag.Mensaje = "Error desconocido en el proceso...";
                }
                return View();

            }
        }

        #endregion







    }
}
