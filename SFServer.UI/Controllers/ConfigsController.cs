using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Configs;
using SFServer.Shared.Client.Configs;
using SFServer.UI.Models;
using System;

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
                Configs = response?.Configs ?? new List<ConfigMetadata>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ConfigsViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Version) || model.Files.Length == 0)
            {
                TempData["Error"] = "Version and config files are required.";
                return RedirectToAction("Index");
            }

            using var client = GetAuthenticatedHttpClient();
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Version), "Version");
            content.Add(new StringContent(model.Environment.ToString()), "Environment");
            foreach (var file in model.Files)
            {
                content.Add(new StreamContent(file.OpenReadStream()), "Files", file.FileName);
            }

            await client.PostAsync("Configs", content);
            TempData["Success"] = "Config uploaded.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var client = GetAuthenticatedHttpClient();
            await client.DeleteAsync($"Configs/{id}");
            TempData["Success"] = "Config deleted.";
            return RedirectToAction("Index");
        }
    }
}
