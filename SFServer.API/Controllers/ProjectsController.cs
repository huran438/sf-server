using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Project;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin,Developer")]
public class ProjectsController : ControllerBase
{
    private readonly DatabseContext _db;

    public ProjectsController(DatabseContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Projects.ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ProjectInfo project)
    {
        project.Id = Guid.NewGuid();
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project);
    }
}
