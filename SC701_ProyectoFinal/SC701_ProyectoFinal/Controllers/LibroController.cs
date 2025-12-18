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
                var rol = HttpContext.Session.GetInt32("Id_Rol");
                var urlApi = "";
                if(rol == 2)
                {
                    urlApi = _configuration["Valores:UrlAPI"] + "Libro/ListarLibrosCliente";
                }
                else
                {
                    urlApi = _configuration["Valores:UrlAPI"] + "Libro/ListarLibros";
                }

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
            ViewBag.Categorias = await ObtenerCategoriasAsync();
            ViewBag.Estados = await ObtenerEstadosAsync();
            return View();
        }

        // Crear libro - POST
        [HttpPost]
        public async Task<IActionResult> Create(LibroModel libro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategoriasAsync();
                return View(libro);
            }

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
                    Descripcion = libro.Descripcion,
                    Anio = libro.Anio,
                    Imagen_URL = libro.Imagen_URL,
                    Id_Categoria = libro.Id_Categoria
                };

                var respuesta = client.PostAsJsonAsync(urlApi, request).Result;

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ViewBag.Mensaje = "No se pudo registrar el libro";
                ViewBag.Categorias = await ObtenerCategoriasAsync();
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

                ViewBag.Categorias = await ObtenerCategoriasAsync();
                return View(libro);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, LibroModel libro)
        {
            if (!ModelState.IsValid)
            {                
                ViewBag.Categorias = await ObtenerCategoriasAsync();
                return View(libro);
            }

            
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
                    Descripcion = libro.Descripcion,
                    Anio = libro.Anio,
                    Imagen_URL = libro.Imagen_URL,
                    Id_Categoria = libro.Id_Categoria,
                };

                var respuesta = client.PutAsJsonAsync(urlApi, request).Result;

                if (respuesta.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ViewBag.Mensaje = "No se pudo actualizar el libro";
                ViewBag.Categorias = await ObtenerCategoriasAsync();
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
                var urlApi = _configuration["Valores:UrlAPI"] + $"Libro/{id}";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var response = await client.DeleteAsync(urlApi);

                if (!response.IsSuccessStatusCode)
                {
                    var mensaje = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = mensaje;
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Libro eliminado correctamente.";
                return RedirectToAction("Index");
            }
        }

        // Vista detalle libro con comentarios
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                // 1. Obtener libro
                var urlLibro = _configuration["Valores:UrlAPI"] + $"Libro/{id}";
                var responseLibro = await client.GetAsync(urlLibro);

                if (!responseLibro.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "No se pudo cargar el libro.";
                    return RedirectToAction("Index");
                }

                var libro = await responseLibro.Content.ReadFromJsonAsync<LibroViewModel>();

                // 2. Obtener comentarios del libro
                var urlComentarios = _configuration["Valores:UrlAPI"] + $"Comentarios/Libro/{id}";
                var responseComentarios = await client.GetAsync(urlComentarios);

                if (responseComentarios.IsSuccessStatusCode)
                {
                    libro.Comentarios =
                        await responseComentarios.Content.ReadFromJsonAsync<List<ComentarioViewModel>>();
                }
                else
                {
                    libro.Comentarios = new List<ComentarioViewModel>();
                }

                return View("DetalleLibro", libro);
            }
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
                    Id_Libro = libro.Id_Libro,
                    Titulo = libro.Titulo,
                    FechaReserva = DateTime.Now.Date,
                    FechaVencimiento = DateTime.Now.Date
                });
            }

        }


        // POST: Confirmar reserva
        [HttpPost]
        public IActionResult Reservar(ReservaViewModel reserva) //NO LLEGA ID LIBRO
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
                    Id_Libro = reserva.Id_Libro,
                    Id_Usuario = idUsuario.Value,
                    Fecha = reserva.FechaReserva,
                    Fecha_Vencimiento = reserva.FechaVencimiento
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

        //OBTENER CATEGORIAS
        private async Task<List<CategoriaModel>> ObtenerCategoriasAsync()
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Categoria/ObtenerCategorias";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    return await respuesta.Content.ReadFromJsonAsync<List<CategoriaModel>>();
                }

                return new List<CategoriaModel>();
            }
        }

    }

}
