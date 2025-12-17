using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    public class ReservaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ReservaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var IdUsuario = HttpContext.Session.GetInt32("Id_Usuario");

                if (IdUsuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validar sanción
                var urlSancion = _configuration["Valores:UrlAPI"] + "Sancion/UsuarioTieneSancion";
                context.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respSancion = context.GetAsync(urlSancion).Result;
                bool tieneSancion = false;

                if (respSancion.IsSuccessStatusCode)
                {
                    var numero = respSancion.Content.ReadAsStringAsync().Result;
                    if (int.TryParse(numero, out int result))
                    {
                        tieneSancion = result > 0;
                    }
                }

                ViewBag.tieneSancion = tieneSancion;

                // Obtener reservas del usuario
                var urlApi = _configuration["Valores:UrlAPI"] +
                             "Reserva/ObtenerReservas?Id_Usuario=" + IdUsuario;

                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi =
                        respuesta.Content.ReadFromJsonAsync<List<ReservaLibroModel>>().Result;

                    var reservasView = datosApi.Select(r => new ReservaViewModel
                    {
                        Id_Movimiento = r.Id_Movimiento,
                        Id_Libro = r.Id_Libro,
                        Titulo = r.Titulo,
                        Imagen_URL = r.Imagen_URL,
                        FechaReserva = r.FechaReserva,
                        FechaVencimiento = r.FechaVencimiento,
                        Estado = r.Estado
                    }).ToList();

                    return View(reservasView);
                }

                ViewBag.Mensaje = "No tienes reservas registradas";
                return View(new List<ReservaViewModel>());
            }
        }


        [HttpPost]
        public async Task<IActionResult> CrearReserva(int LibroId)
        {
            var IdUsuario = HttpContext.Session.GetInt32("Id_Usuario");
            if (IdUsuario == null)
            {
                TempData["Error"] = "Debe iniciar sesión para realizar una reserva.";
                return RedirectToAction("MisReservas", "Reserva");
            }

            var reserva = new
            {
                LibroId,
                UsuarioId = IdUsuario.Value,
                FechaReserva = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(30),
                EstadoReserva = 2
            };

            var context = _httpClientFactory.CreateClient("ProyectoAPI");
            var respuesta = await context.PostAsJsonAsync("reserva/crear", reserva);

            if (respuesta.IsSuccessStatusCode)
                TempData["Mensaje"] = "Reserva creada exitosamente.";
            else
                TempData["Error"] = "Error al crear la reserva. Inténtelo de nuevo.";

            return RedirectToAction("MisReservas");
        }

        [HttpGet]
        public async Task<IActionResult> MisReservas()
        {
            var IdUsuario = HttpContext.Session.GetInt32("Id_Usuario");
            if (IdUsuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }
            var Client = _httpClientFactory.CreateClient("ProyectoAPI");

            var respuesta = await Client.GetAsync($"reserva/usuario/{IdUsuario.Value}");

            if (!respuesta.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron cargar las reservas.";

                return View(new List<ReservaViewModel>());
            }

            var reservas = await respuesta.Content.ReadFromJsonAsync<List<ReservaViewModel>>();

            return View(reservas);

        }

        [HttpPost]
        public IActionResult CancelarReserva(ReservaLibroModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                int Id_rol = (int)HttpContext.Session.GetInt32("Id_Rol")!;

                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/CancelarReserva";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, reserva).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return Id_rol == 1 ? RedirectToAction("ObtenerReservas") : RedirectToAction("Index");
                        
                    }
                }                
                return RedirectToAction("ObtenerReservas");
            }
        }

        [HttpPost]
        public IActionResult CambiarEstadoReserva(ReservaLibroModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/CambiarEstadoReserva"; 
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, reserva).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("ObtenerReservas");
                    }
                }
                return RedirectToAction("ObtenerReservas");
            }
        }

        [HttpPost]
        public IActionResult CambiarEstadoCompletado(ReservaLibroModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/CambiarEstadoCompletado";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, reserva).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("ObtenerReservas");
                    }
                }
                return RedirectToAction("ObtenerReservas");
            }
        }

        [HttpPost]
        public IActionResult ExtenderPlazoPrestamo(ReservaLibroModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/ExtenderPrestamo";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, reserva).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult ObtenerReservas(int? estado)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/ObtenerReservasAdmin";

                if (estado != null)
                {
                    urlApi += "?estado=" + estado;
                }

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var reservas =
                        respuesta.Content.ReadFromJsonAsync<List<ReservaListadoViewModel>>().Result;

                    return View(reservas);
                }

                ViewBag.Mensaje = "No hay reservas registradas";
                return View(new List<ReservaListadoViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            var response = await client.DeleteAsync($"reserva/cancelar/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Reserva cancelada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo cancelar la reserva.";
            }

            return RedirectToAction("ObtenerReservas");
        }

    }

}

        