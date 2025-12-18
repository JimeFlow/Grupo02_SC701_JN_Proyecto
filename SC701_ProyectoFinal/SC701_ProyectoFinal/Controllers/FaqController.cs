using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using static System.Net.WebRequestMethods;

namespace SC701_ProyectoFinal.Controllers
{
    public class FAQController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FAQController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Vista pública
        public async Task<IActionResult> Index()
        {
            using var client = _httpClientFactory.CreateClient();
            var url = _configuration["Valores:UrlAPI"] + "FAQ/Activas";

            var response = await client.GetAsync(url);

            if(!response.IsSuccessStatusCode)
            {
                // Manejar el error según sea necesario
                return View(new List<FAQViewModel>());
            }
            var faqs = await response.Content.ReadFromJsonAsync<List<FAQViewModel>>();

            return View(faqs);
        }

        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            using var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] + "FAQ/Admin";

            var response = await client.GetAsync(url);

            if(!response.IsSuccessStatusCode)
            {
                // Manejar el error según sea necesario
                return View(new List<FAQViewModel>());
            }
            var faqs = await response.Content.ReadFromJsonAsync<List<FAQViewModel>>();

            return View(faqs);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(FAQRequestModel model)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] + "FAQ/Crear";
            await client.PostAsJsonAsync(url, model);

            return RedirectToAction("Admin");
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int idFAQ, bool estado)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] + $"FAQ/CambiarEstado?idFAQ={idFAQ}&estado={estado}";
            await client.PutAsync(url, null);

            return RedirectToAction("Admin");
        }
    }

}
