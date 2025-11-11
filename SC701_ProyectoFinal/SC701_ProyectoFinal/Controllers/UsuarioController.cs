using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    [SeguridadPorRol]
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

        #region Registro de usuarios admin
        [HttpGet]
        public async Task<IActionResult>RegistrarUsuarioAdmin()
        {
            ViewBag.Roles = await ObtenerRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarUsuarioAdmin(UsuarioModel usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRoles();
                return View(usuario);
            }
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
                        return RedirectToAction("Index");
                    }
                }

                ViewBag.Mensaje = "No se ha registrado la información" + respuesta;
                ViewBag.Roles = await ObtenerRoles();
                return View(usuario);
            }
        }
        #endregion

        #region ActualizarUsuario

        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int id)
        {
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ObtenerUsuario/" + id;

                context.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await context.GetAsync(urlApi);

                if (!respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var usuario = await respuesta.Content.ReadFromJsonAsync<UsuarioModel>();

                ViewBag.Roles = await ObtenerRoles();

                return View("EditarUsuario", usuario);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(UsuarioModel usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRoles();
                return View("EditarUsuario", usuario);
            }

            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/EditarUsuario";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        ViewBag.Mensaje = "La información se ha actualizado correctamente";
                    }
                    else
                    {
                        ViewBag.Mensaje = "La información no se ha actualizado correctamente";
                    }
                }                
                ViewBag.Roles = await ObtenerRoles();
                return View("EditarUsuario", usuario);
            }
        }

        #endregion

        #region Eliminar usuario admin

        [HttpGet]
        public IActionResult EliminarUsuario(int id)
        {
            using (var context = _http.CreateClient())
            {
                var urlApi = $"{ _configuration["Valores:UrlAPI"]}Usuario/EliminarUsuario/{id}";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.DeleteAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {                   
                    RedirectToAction("Index");                    
                }
                ViewBag.Mensaje = "Error al eliminar el usuario";
                return RedirectToAction("Index");
            }
        }

        #endregion


        #region Obtener Roles
        private async Task<SelectList> ObtenerRoles()
        {
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ListarRoles";

                context.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await context.GetAsync(urlApi);

                if (!respuesta.IsSuccessStatusCode)
                    return new SelectList(new List<RolModel>(), "Id_Rol", "Tipo_Rol");

                var datos = await respuesta.Content.ReadFromJsonAsync<List<RolModel>>();

                return new SelectList(datos, "Id_Rol", "Tipo_Rol");
            }
        }


        #endregion


    }
}
