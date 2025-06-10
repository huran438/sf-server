using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Configs;
using SFServer.Shared.Client.Configs;
using SFServer.UI.Models;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class ConfigsController : Controller
    {
        private readonly IConfiguration _configuration;

        public ConfigsController(IConfiguration configuration)
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
            var response = await client.GetFromMessagePackAsync<GetAllConfigsResponse>("Configs");
            var model = new ConfigsViewModel
            {
                Configs = response?.Configs.Select(c => c.Metadata).ToList() ?? new List<ConfigMetadata>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ConfigsViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Version) || string.IsNullOrWhiteSpace(model.ConfigJson))
            {
                TempData["Error"] = "Version and config JSON are required.";
                return RedirectToAction("Index");
            }

            var request = new UploadConfigRequest
            {
                Version = model.Version,
                Environment = model.Environment,
                Config = model.ConfigJson
            };

            using var client = GetAuthenticatedHttpClient();
            await client.PostAsMessagePackAsync<UploadConfigRequest, ConfigMetadata>("Configs", request);
            TempData["Success"] = "Config uploaded.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string version, ConfigEnvironment environment)
        {
            using var client = GetAuthenticatedHttpClient();
            await client.DeleteAsync($"Configs/{version}/{environment}");
            TempData["Success"] = "Config deleted.";
            return RedirectToAction("Index");
        }
    }
}
