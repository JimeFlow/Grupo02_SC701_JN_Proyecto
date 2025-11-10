using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

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
                        HttpContext.Session.SetString("Nombre", datosApi.Nombre);
                        HttpContext.Session.SetString("Tipo_Rol", datosApi.Tipo_Rol);
                        HttpContext.Session.SetInt32("Id_Usuario", datosApi.Id_Usuario);
                        HttpContext.Session.SetInt32("Id_Rol", datosApi.Id_Rol);
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

        #region Recuperar Acceso

        [HttpPost]
        public IActionResult RecuperarAcceso(UsuarioModel usuario)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] +
                             "Account/ValidarUsuario?Correo=" + usuario.Correo;

                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                    if (datosApi != null)
                        return RedirectToAction("Login", "Account");
                }

                ViewBag.Mensaje = "No se ha recuperado el acceso";
                return View();
            }
        }

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        #endregion


        #region EditarPerfil
        [Seguridad]
        [HttpGet]
        public IActionResult EditarPerfil()
        {
            return View();
        }

        [HttpPost]
        [Seguridad]
        public IActionResult EditarPerfil(UsuarioModel usuario)
        {
            ViewBag.Mensaje = "La información no se ha actualizado correctamente";
            usuario.Id_Usuario = (int)HttpContext.Session.GetInt32("Id_Usuario")!;

            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ActualizarPerfil";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        ViewBag.Mensaje = "La información se ha actualizado correctamente";
                        //setear las sessions si hacen falta (todas) cuando se tenga la vista lista
                        HttpContext.Session.SetString("NombreUsuario", usuario.Nombre);
                    }
                }

                return View();
            }
        }

        #endregion


        //pasar a account
        #region ActualizarSeguridad
        [HttpGet]
        [Seguridad]
        public IActionResult ActualizarSeguridad()
        {
            return View();
        }

        [HttpPost]
        [Seguridad]
        public IActionResult ActualizarSeguridad(UsuarioModel usuario)
        {
            ViewBag.Mensaje = "La información no se ha actualizado correctamente";
            usuario.Id_Usuario = (int)HttpContext.Session.GetInt32("Id_Usuario")!;
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ActualizarSeguridad";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                        ViewBag.Mensaje = "La información se ha actualizado correctamente";
                }

                return View();
            }
        }
        #endregion

        //cerrar sesion
        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }



    }
}
