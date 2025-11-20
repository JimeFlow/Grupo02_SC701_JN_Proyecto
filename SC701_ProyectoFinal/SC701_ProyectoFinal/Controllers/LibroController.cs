using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    public class LibroController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LibroController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Lista de libros
        public IActionResult Index()
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Libro/ListarLibros";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var libros = respuesta.Content.ReadFromJsonAsync<List<LibroModel>>().Result;
                    return View(libros);
                }

                ViewBag.Mensaje = "No hay libros registrados";
                return View(new List<LibroModel>());
            }
        }

        // Crear libro - GET
        [HttpGet]
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

            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Libro/RegistrarLibro";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

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

                var respuesta = client.PostAsJsonAsync(urlApi, request).Result;

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ViewBag.Mensaje = "No se pudo registrar el libro";
                ViewBag.Estados = await ObtenerEstadosAsync();
                return View(libro);
            }
        }

        // Editar libro - GET
        public async Task<IActionResult> Edit(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + $"Libro/{id}";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(urlApi).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var libro = respuesta.Content.ReadFromJsonAsync<LibroModel>().Result;

                ViewBag.Estados = await ObtenerEstadosAsync();
                return View(libro);
            }
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

            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + $"Libro/ActualizarLibro/{id}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

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

                var respuesta = client.PutAsJsonAsync(urlApi, request).Result;

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ViewBag.Mensaje = "No se pudo actualizar el libro";
                ViewBag.Estados = await ObtenerEstadosAsync();
                return View(libro);
            }
        }

        // Eliminar libro - GET (confirmación)
        public async Task<IActionResult> Delete(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + $"Libro/{id}";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(urlApi).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var libro = respuesta.Content.ReadFromJsonAsync<LibroModel>().Result;
                return View(libro);
            }
        }

        // Eliminar libro - POST
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + $"Libro/EliminarLibro/{id}";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.DeleteAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                TempData["ErrorMessage"] = "No se pudo eliminar el libro";
                return RedirectToAction("Delete", new { id });
            }

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
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Estado";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.GetAsync(urlApi);

                if (!respuesta.IsSuccessStatusCode)
                    return new List<EstadoModel>();

                return await respuesta.Content.ReadFromJsonAsync<List<EstadoModel>>();
            }
        }

        //get
        public IActionResult Reservar(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + $"Libro/{id}";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(urlApi).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var libro = respuesta.Content.ReadFromJsonAsync<LibroModel>().Result;

                return View(new ReservaViewModel
                {
                    Id = libro.Id_Libro,
                    Titulo = libro.Titulo
                });
            }

        }


        // POST: Confirmar reserva
        [HttpPost]
        public IActionResult Reservar(ReservaViewModel reserva)
        {
            if (!ModelState.IsValid)
                return View(reserva);


            var idUsuario = HttpContext.Session.GetInt32("Id_Usuario");
            if (idUsuario == null)
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para reservar un libro.";
                return RedirectToAction("Login", "Usuario");
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Libro/ReservarLibro";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var request = new ReservaRequestModel
                {
                    Id_Libro = reserva.Id,
                    Id_Usuario = idUsuario.Value,
                    Tipo = "RESERVA",
                    Fecha = reserva.FechaReserva,
                    Fecha_Vencimiento = reserva.FechaVencimiento,
                    Estado = 1
                };

                var respuesta = client.PostAsJsonAsync(urlApi, request).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Reserva creada correctamente";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "No se pudo completar la reserva";
                return View(reserva);
            }
        }
    }

}
