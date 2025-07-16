using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Project;

namespace SFServer.UI.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IConfiguration _config;
    private readonly ProjectContext _context;

    public ProjectsController(IConfiguration config, ProjectContext context)
    {
        _config = config;
        _context = context;
    }

    private HttpClient GetClient() => User.CreateApiClient(_config, _context.CurrentProjectId);

    public async Task<IActionResult> Index()
    {
        using var client = User.CreateApiClient(_config);
        var projects = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects");
        return View(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Select(Guid id)
    {
        using var client = User.CreateApiClient(_config);
        var list = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects");
        var proj = list.FirstOrDefault(p => p.Id == id);
        if (proj != null)
        {
            _context.CurrentProjectId = proj.Id;
            _context.CurrentProjectName = proj.Name;
        }
        return RedirectToAction("Index", "Home");
    }
}
