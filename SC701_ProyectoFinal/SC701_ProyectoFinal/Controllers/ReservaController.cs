using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class ReservaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReservaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

        