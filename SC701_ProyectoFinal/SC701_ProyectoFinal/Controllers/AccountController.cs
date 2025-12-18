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
                        HttpContext.Session.SetString("Token", datosApi.Token);
                        HttpContext.Session.SetString("Correo", datosApi.Correo);
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

            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var urlApi = _configuration["Valores:UrlAPI"] + "Account/Registrarse";


            var respuesta = client.PostAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["RegistroExitoso"] = "Registro exitoso. Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo completar el registro. Verifique los datos ingresados.";
                    return View(usuario);
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
                             "Account/RecuperarAcceso?Correo=" + usuario.Correo;

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
            using (var context = _httpClientFactory.CreateClient())
            {
                var id_usuario = HttpContext.Session.GetInt32("Id_Usuario");
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ObtenerUsuario/" + id_usuario;

                context.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var usuario = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;
                    return View(usuario);
                }
                ViewBag.Mensaje = "No hay información registrada";
                return View(new UsuarioModel());
            }
        }

        [HttpPost]
        [Seguridad]
        public IActionResult EditarPerfil(UsuarioModel usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }
            usuario.Id_Usuario = (int)HttpContext.Session.GetInt32("Id_Usuario")!;

            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Account/ActualizarPerfil";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, usuario).Result; 

                if (respuesta.IsSuccessStatusCode)
                {                    
                    ViewBag.Mensaje = "La información se ha actualizado correctamente";
                    
                    HttpContext.Session.SetString("Nombre", usuario.Nombre);
                    HttpContext.Session.SetString("Tipo_Rol", usuario.Tipo_Rol);
                    HttpContext.Session.SetInt32("Id_Usuario", usuario.Id_Usuario);
                    HttpContext.Session.SetInt32("Id_Rol", usuario.Id_Rol);
                }
                else
                {
                    var error = respuesta.Content.ReadAsStringAsync().Result;
                    if(error != "")
                    {
                        ViewBag.Mensaje = error;
                    }
                    else
                    {
                        ViewBag.Mensaje = "Error desconocido";
                    }
                }
            }
            return View(usuario);
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
                var urlApi = _configuration["Valores:UrlAPI"] + "Account/ActualizarSeguridad";
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
