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
            var s3 = await client.GetFromMessagePackAsync<S3SettingsDto>("ServerSettings/s3");
            var model = new ServerSettingsViewModel { S3 = s3 ?? new S3SettingsDto() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateS3(ServerSettingsViewModel model)
        {
            using var client = GetAuthenticatedHttpClient();
            await client.PostMessagePackAsync("ServerSettings/s3", model.S3);
            TempData["Success"] = "S3 settings updated.";
            return RedirectToAction("Index");
        }
    }
}