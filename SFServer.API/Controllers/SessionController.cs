using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Session;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly DatabseContext _db;

    public SessionController(DatabseContext db)
    {
        _db = db;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] SessionStartDto dto)
    {
        var session = new UserSession
        {
            Id = dto.SessionId,
            UserId = dto.UserId,
            StartTime = DateTime.UtcNow,
            IsPaused = false
        };

        _db.UserSessions.Add(session);
        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpPost("pause")]
    public async Task<IActionResult> PauseSession([FromBody] SessionActionDto dto)
    {
        var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.Id == dto.SessionId && s.UserId == dto.UserId);
        if (session == null)
        {
            return NotFound("Session not found");
        }

        session.IsPaused = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("end")]
    public async Task<IActionResult> EndSession([FromBody] SessionActionDto dto)
    {
        var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.Id == dto.SessionId && s.UserId == dto.UserId);
        if (session == null)
        {
            return NotFound("Session not found");
        }

        session.EndTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }
}
