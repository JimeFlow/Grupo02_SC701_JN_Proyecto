using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;
using System.Net.Http.Headers;

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
            var faqs = await response.Content.ReadFromJsonAsync<List<FAQViewModel>>();

            return View(faqs);
        }
    }
}
