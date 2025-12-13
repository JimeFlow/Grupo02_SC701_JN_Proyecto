using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;


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

            var comentariosResponse = await client.GetAsync($"/api/Comentarios/ListarPorLibro/{id}");
            var comentarios = new List<ComentarioViewModel>();

            if (comentariosResponse.IsSuccessStatusCode)
            {
                comentarios = await comentariosResponse.Content.ReadFromJsonAsync<List<ComentarioViewModel>>();
            }

            libro.Comentarios = comentarios;
            return View(libro);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarComentario(ComentarioModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("Id_Usuario");

            if (usuarioId == null)
            {
                TempData["ErrorComentario"] = "Debe iniciar sesión para comentar.";
                return RedirectToAction("Detalle", new { id = model.Id_Libro });
            }

            model.Id_Usuario = usuarioId.Value;

            var client = _httpClientFactory.CreateClient("ProyectoAPI");
            var response = await client.PostAsJsonAsync("/api/Comentarios/Crear", model);

            if (response.IsSuccessStatusCode)
                TempData["MensajeComentario"] = "Comentario agregado correctamente.";
            else
                TempData["ErrorComentario"] = "Ocurrió un error al agregar el comentario.";

            return RedirectToAction("Detalle", new { id = model.Id_Libro });
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
                    LibroId = libro.Id_Libro,
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
                    Id_Libro = reserva.LibroId,
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
