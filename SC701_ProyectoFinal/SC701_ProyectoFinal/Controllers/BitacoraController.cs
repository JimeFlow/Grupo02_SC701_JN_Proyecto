using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class BitacoraController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public BitacoraController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        [Seguridad] // tu filtro
        public IActionResult Index()
        {
            using var client = _http.CreateClient();

            var urlApi = _configuration["Valores:UrlAPI"] + "Bitacora/ObtenerLogs";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    HttpContext.Session.GetString("Token"));

            var respuesta = client.GetAsync(urlApi).Result;

            if (respuesta.IsSuccessStatusCode)
            {
                var datos = respuesta.Content
                    .ReadFromJsonAsync<List<BitacoraModel>>().Result;

                return View(datos);
            }

            return View(new List<BitacoraModel>());
        }
    }
}