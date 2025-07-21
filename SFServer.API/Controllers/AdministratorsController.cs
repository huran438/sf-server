using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Admin;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class AdministratorsController : ControllerBase
{
    private readonly DatabseContext _db;
    private readonly IPasswordHasher<Administrator> _hasher;

    public AdministratorsController(DatabseContext db, IPasswordHasher<Administrator> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var admins = await _db.Administrators.ToListAsync();
        return Ok(admins);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _db.Administrators.AnyAsync(a => a.Username.ToLower() == request.Username.ToLower()))
            return BadRequest("Username already exists.");
        if (!string.IsNullOrEmpty(request.Email) && await _db.Administrators.AnyAsync(a => a.Email.ToLower() == request.Email.ToLower()))
            return BadRequest("Email already exists.");

        var admin = new Administrator
        {
            Id = Guid.CreateVersion7(),
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = _hasher.HashPassword(admin, request.Password);
        _db.Administrators.Add(admin);
        await _db.SaveChangesAsync();
        return Ok(admin);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty) return BadRequest();
        var admin = await _db.Administrators.FindAsync(id);
        if (admin == null) return NotFound();
        _db.Administrators.Remove(admin);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
