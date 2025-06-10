using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SFServer.UI.Models;
using SFServer.Shared.Client.ServerSettings;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : Controller
    {
        private readonly IConfiguration _configuration;

        public ServerSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private HttpClient GetAuthenticatedHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_configuration["API_BASE_URL"]) };
            var jwtToken = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }
            return client;
        }

        public async Task<IActionResult> Index()
        {
            using var client = GetAuthenticatedHttpClient();
            var settings = await client.GetFromMessagePackAsync<ServerSettingsDto>("ServerSettings");
            var model = new ServerSettingsViewModel { Settings = settings ?? new ServerSettingsDto() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ServerSettingsViewModel model)
        {
            using var client = GetAuthenticatedHttpClient();
            await client.PostMessagePackAsync("ServerSettings", model.Settings);
            TempData["Success"] = "Server settings updated.";
            return RedirectToAction("Index");
        }
    }
}