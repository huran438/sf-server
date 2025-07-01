using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Settings;
using SFServer.UI.Models;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ServerSettingsService _service;
        public ServerSettingsController(IConfiguration configuration, ServerSettingsService service)
        {
            _configuration = configuration;
            _service = service;
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
            var settings = await client.GetFromMessagePackAsync<ServerSettings>("ServerSettings");
            if (settings != null)
                _service.UpdateCache(settings);
            var vm = new ServerSettingsViewModel
            {
                Id = settings.Id,
                ServerCopyright = settings.ServerCopyright,
                GoogleClientId = settings.GoogleClientId,
                GoogleClientSecret = settings.GoogleClientSecret
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ServerSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var client = GetAuthenticatedHttpClient();
            var payload = new ServerSettings
            {
                Id = model.Id,
                ServerCopyright = model.ServerCopyright,
                GoogleClientId = model.GoogleClientId,
                GoogleClientSecret = model.GoogleClientSecret
            };
            var response = await client.PutAsMessagePackAsync("ServerSettings", payload);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to save settings.";
            }
            else
            {
                TempData["Success"] = "Settings saved.";
                _service.UpdateCache(payload);
            }
            return RedirectToAction("Index");
        }
    }
}