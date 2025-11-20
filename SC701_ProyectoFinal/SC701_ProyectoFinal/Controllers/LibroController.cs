using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SC701_ProyectoFinal.Models;
using System.Text;

namespace SC701_ProyectoFinal.Controllers
{
    public class LibroController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

    public LibroController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Lista de libros
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.GetAsync("Libro");

            if (!response.IsSuccessStatusCode)
                return View(new List<LibroModel>());

            var json = await response.Content.ReadAsStringAsync();
            var libros = JsonConvert.DeserializeObject<List<LibroModel>>(json);
            return View(libros);
        }

        // Crear libro - GET
        public async Task<IActionResult> Create()
        {
            ViewBag.Estados = await ObtenerEstadosAsync();
            return View();
        }

        // Crear libro - POST
        [HttpPost]
        public async Task<IActionResult> Create(LibroModel libro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Estados = await ObtenerEstadosAsync();
                return View(libro);
            }

            // Llenar Estado_Libro según Id_Estado
            var estados = await ObtenerEstadosAsync();
            var estadoSeleccionado = estados.FirstOrDefault(e => e.Id_Estado == libro.Id_Estado);
            libro.Estado_Libro = estadoSeleccionado?.Estado;

            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            var request = new LibroRequestModel
            {
                ISBN = libro.ISBN,
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                Anio = libro.Anio,
                Imagen_URL = libro.Imagen_URL,
                Id_Estado = libro.Id_Estado,
                Estado_Libro = libro.Estado_Libro
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Libro/RegistrarLibro", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "No se pudo registrar el libro");
            ViewBag.Estados = await ObtenerEstadosAsync();
            return View(libro);
        }

        // Editar libro - GET
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.GetAsync($"Libro/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await response.Content.ReadAsStringAsync();
            var libro = JsonConvert.DeserializeObject<LibroModel>(json);

            if (libro == null)
                return NotFound();

            ViewBag.Estados = await ObtenerEstadosAsync();
            return View(libro);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, LibroModel libro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Estados = await ObtenerEstadosAsync();
                return View(libro);
            }

            // Llenar Estado_Libro según Id_Estado
            var estados = await ObtenerEstadosAsync();
            var estadoSeleccionado = estados.FirstOrDefault(e => e.Id_Estado == libro.Id_Estado);
            libro.Estado_Libro = estadoSeleccionado?.Estado;

            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            var json = JsonConvert.SerializeObject(new LibroRequestModel
            {
                ISBN = libro.ISBN,
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                Anio = libro.Anio,
                Imagen_URL = libro.Imagen_URL,
                Id_Estado = libro.Id_Estado,
                Estado_Libro = libro.Estado_Libro
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"Libro/ActualizarLibro/{id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "No se pudo actualizar el libro");
            ViewBag.Estados = await ObtenerEstadosAsync();
            return View(libro);
        }

        // Eliminar libro - GET (confirmación)
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.GetAsync($"Libro/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await response.Content.ReadAsStringAsync();
            var libro = JsonConvert.DeserializeObject<LibroModel>(json);

            if (libro == null)
                return RedirectToAction(nameof(Index));

            return View(libro);
        }

        // Eliminar libro - POST
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.DeleteAsync($"Libro/EliminarLibro/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "No se pudo eliminar el libro.");
            return RedirectToAction(nameof(Delete), new {id});
        }

        // Vista detalle libro con comentarios
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            var response = await client.GetAsync($"/api/Libro/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "No se puede cargar el libro.";
                return RedirectToAction("Index", "Libro");
            }

            var libro = await response.Content.ReadFromJsonAsync<LibroViewModel>();

            var comentariosResponse = await client.GetAsync($"/api/Comentarios/Libro/{id}");
            var comentarios = new List<ComentarioViewModel>();

            if (comentariosResponse.IsSuccessStatusCode)
            {
                comentarios = await comentariosResponse.Content.ReadFromJsonAsync<List<ComentarioViewModel>>();
            }

            libro.Comentarios = comentarios;
            return View(libro);
        }

        // Enviar comentario
        [HttpPost]
        public async Task<IActionResult> EnviarComentario(ComentarioModel comentario)
        {
            var UsuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (UsuarioId == null)
            {
                return RedirectToAction("DetalleLibro", new { id = comentario.LibroId });
            }

            comentario.UsuarioId = UsuarioId.Value;

            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.PostAsJsonAsync("/api/Comentarios/Crear", comentario);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Comentario enviado exitosamente.";
            else
                TempData["ErrorMessage"] = "Error al enviar el comentario.";

            return RedirectToAction("DetalleLibro", new { id = comentario.LibroId });
        }

        // Obtener estados para select
        private async Task<List<EstadoModel>> ObtenerEstadosAsync()
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.GetAsync("Estado");

            if (!response.IsSuccessStatusCode)
                return new List<EstadoModel>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<EstadoModel>>(json);
        }

        // GET: Mostrar formulario de reserva
        // Solo abre la vista de reservar
        public async Task<IActionResult> Reservar(int id)
        {
            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            // Traer los datos del libro desde el API
            var response = await client.GetAsync($"Libro/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "No se pudo cargar el libro.";
                return RedirectToAction(nameof(Index));
            }

            var libro = await response.Content.ReadFromJsonAsync<LibroModel>();

            // Crear el ViewModel para la vista de reserva
            var reserva = new ReservaViewModel
            {
                Id = libro.Id_Libro,
                Titulo = libro.Titulo,
            };

            return View(reserva); // Abre la vista Reservar.cshtml

        }


        // POST: Confirmar reserva
        [HttpPost]
        public async Task<IActionResult> Reservar(ReservaViewModel reserva)
        {
            if (!ModelState.IsValid)
                return View(reserva);

            var client = _httpClientFactory.CreateClient("ProyectoAPI");

            var idUsuario = HttpContext.Session.GetInt32("Id_Usuario");
            if (idUsuario == null)
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para reservar un libro.";
                return RedirectToAction("Login", "Usuario");
            }

            var request = new ReservaRequestModel
            {
                Id_Libro = reserva.Id,
                Id_Usuario = idUsuario.Value,
                Tipo = "RESERVA",
                Fecha = reserva.FechaReserva,
                Fecha_Vencimiento = reserva.FechaVencimiento,
                Estado = 1
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Libro/ReservarLibro", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Reserva creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "No se pudo crear la reserva.";
            return View(reserva);
        }
    }

}
