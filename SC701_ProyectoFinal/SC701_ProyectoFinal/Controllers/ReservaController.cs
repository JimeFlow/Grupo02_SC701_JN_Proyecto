using Microsoft.AspNetCore.Mvc;

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
                EstadoReserva = 1
            };

            // Using the API client to create the reservation
            var context = _httpClientFactory.CreateClient("API");
            var respuesta = await context.PostAsJsonAsync("reservas/crear", reserva);

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
                return RedirectToAction("Login", "Account");

            // Using the API client to get the user's reservations
            var context = _httpClientFactory.CreateClient("API");
            var respuesta = await context.GetAsync($"reservas/usuario/{IdUsuario.Value}");

            if (respuesta.IsSuccessStatusCode)
            {
                var reservas = await respuesta.Content.ReadFromJsonAsync<List<Models.ReservaViewModel>>();
                return View(reservas);
            }

            TempData["Error"] = "Error al obtener las reservas.";
            return View(new List<Models.ReservaViewModel>());
        }
    }
}