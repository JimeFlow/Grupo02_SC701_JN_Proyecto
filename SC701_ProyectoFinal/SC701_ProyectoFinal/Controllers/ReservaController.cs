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
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/ObtenerReservas?Id_Usuario="+IdUsuario;
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<List<ReservaRequestModel>>().Result;
                    return View(datosApi);
                }

                ViewBag.Mensaje = "No hay productos registrados";
                return View(new List<UsuarioModel>());
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
        public IActionResult CancelarReserva(ReservaRequestModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/CancelarReserva";
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

        [HttpPost]
        public IActionResult CambiarEstadoReserva(ReservaRequestModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/CambiarEstadoReserva"; //admin -> de pendiente a reservado
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
        public IActionResult CambiarEstadoCancelacion(ReservaRequestModel reserva)
        {
            using (var context = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/CancelarEstadoCancelado";//si
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
        public IActionResult ExtenderPlazoPrestamo(ReservaRequestModel reserva)
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
            using (var context = _httpClientFactory.CreateClient())
            {
                var IdUsuario = HttpContext.Session.GetInt32("Id_Usuario");
                var urlApi = _configuration["Valores:UrlAPI"] + "Reserva/ObtenerReservasAdmin?estado=" + estado;
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<List<ReservaRequestModel>>().Result;
                    return View(datosApi);
                }

                ViewBag.Mensaje = "No hay productos registrados";
                return View(new List<ReservaRequestModel>());
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

            return RedirectToAction("MisReservas");
        }

    }

}

        