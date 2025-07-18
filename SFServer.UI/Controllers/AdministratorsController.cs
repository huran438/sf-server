using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Admin;

namespace SFServer.UI.Controllers;

[Authorize(Roles = "Admin")]
public class AdministratorsController : Controller
{
    private readonly IConfiguration _config;
    private readonly ProjectContext _project;

    public AdministratorsController(IConfiguration config, ProjectContext project)
    {
        _config = config;
        _project = project;
    }

    private HttpClient GetClient() => User.CreateApiClient(_config);

    public async Task<IActionResult> Index()
    {
        using var client = GetClient();
        var admins = await client.GetFromMessagePackAsync<List<Administrator>>("Administrators");
        return View(admins);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAdminRequest model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index), new { projectId = _project.CurrentProjectId });

        using var client = GetClient();
        await client.PostAsMessagePackAsync<CreateAdminRequest, Administrator>("Administrators", model);
        return RedirectToAction(nameof(Index), new { projectId = _project.CurrentProjectId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        using var client = GetClient();
        await client.DeleteAsync($"Administrators/{id}");
        return RedirectToAction(nameof(Index), new { projectId = _project.CurrentProjectId });
    }
}
