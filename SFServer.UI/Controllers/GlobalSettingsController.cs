using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Settings;
using SFServer.Shared.Server.Admin;
using SFServer.Shared.Server.Project;
using SFServer.UI.Models;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GlobalSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly GlobalSettingsService _service;

        public GlobalSettingsController(IConfiguration configuration, GlobalSettingsService service)
        {
            _configuration = configuration;
            _service = service;
        }

        private HttpClient GetClient() => User.CreateApiClient(_configuration);

        public async Task<IActionResult> Index()
        {
            using var client = GetClient();
            var settings = await client.GetFromMessagePackAsync<GlobalSettings>("GlobalSettings") ?? new GlobalSettings();
            _service.UpdateCache(settings);

            var admins = await client.GetFromMessagePackAsync<List<Administrator>>("Administrators");
            var projects = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects");

            var model = new GlobalSettingsViewModel
            {
                Settings = settings,
                Administrators = admins ?? new(),
                Projects = projects ?? new()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(GlobalSettings settings)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            using var client = GetClient();
            var response = await client.PutAsMessagePackAsync("GlobalSettings", settings);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Settings saved.";
                _service.UpdateCache(settings);
            }
            else
            {
                TempData["Error"] = "Failed to save settings.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(CreateAdminRequest request)
        {
            using var client = GetClient();
            await client.PostAsMessagePackAsync<CreateAdminRequest, Administrator>("Administrators", request);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdmin(Guid id)
        {
            using var client = GetClient();
            await client.DeleteAsync($"Administrators/{id}");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(string name)
        {
            using var client = GetClient();
            await client.PostAsMessagePackAsync<ProjectInfo, ProjectInfo>("Projects", new ProjectInfo { Name = name });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RenameProject(Guid id, string name)
        {
            using var client = GetClient();
            await client.PutAsMessagePackAsync($"Projects/{id}", new ProjectInfo { Id = id, Name = name });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            using var client = GetClient();
            await client.DeleteAsync($"Projects/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
