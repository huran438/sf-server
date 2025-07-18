using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Project;
using SFServer.UI;

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

    private HttpClient GetClient(Guid projectId = default) => User.CreateApiClient(_config, projectId);

    public async Task<IActionResult> Index()
    {
        using var client = GetClient();
        var projects = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects");
        return View(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Select(Guid id)
    {
        using var client = GetClient();
        var list = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects");
        var proj = list.FirstOrDefault(p => p.Id == id);
        if (proj != null)
        {
            _context.CurrentProjectId = proj.Id;
            _context.CurrentProjectName = proj.Name;
        }
        return RedirectToAction("Index", "Home", new { projectId = _context.CurrentProjectId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(string name)
    {
        using var client = GetClient();
        var project = new ProjectInfo { Name = name };
        await client.PostAsMessagePackAsync<ProjectInfo, ProjectInfo>("Projects", project);
        return RedirectToAction(nameof(Index), new { projectId = _context.CurrentProjectId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Rename(Guid id, string name)
    {
        using var client = GetClient();
        var project = new ProjectInfo { Id = id, Name = name };
        await client.PutAsMessagePackAsync($"Projects/{id}", project);
        if (_context.CurrentProjectId == id)
        {
            _context.CurrentProjectName = name;
        }
        return RedirectToAction(nameof(Index), new { projectId = _context.CurrentProjectId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return RedirectToAction(nameof(Index), new { projectId = _context.CurrentProjectId });

        using var client = GetClient();
        await client.DeleteAsync($"Projects/{id}");
        if (_context.CurrentProjectId == id)
        {
            _context.CurrentProjectId = Guid.Empty;
            _context.CurrentProjectName = string.Empty;
        }
        return RedirectToAction(nameof(Index), new { projectId = _context.CurrentProjectId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Settings(Guid id)
    {
        using var client = GetClient();
        var list = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects");
        var proj = list.FirstOrDefault(p => p.Id == id);
        if (proj != null)
        {
            _context.CurrentProjectId = proj.Id;
            _context.CurrentProjectName = proj.Name;
        }
        return RedirectToAction("Index", "ProjectSettings", new { projectId = _context.CurrentProjectId });
    }
}
