using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Client.Base;
using SFServer.Shared.Client.Session;

namespace SFServer.API.Controllers {
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class SessionController : ControllerBase {
        private readonly DatabseContext _db;

        public SessionController(DatabseContext db) {
            _db = db;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromHeader(Name = Headers.UID)] Guid userId, [FromBody] StartSessionRequest dto) {
            var session = new UserSession
            {
                Id = dto.SessionId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                ResumeCounter = 0
            };

            if (_db.UserSessions.Any(s => s.Id == session.Id))
            {
                return BadRequest("Session already exists");
            }

            await _db.UserSessions.AddAsync(session);
            await _db.SaveChangesAsync();
            return Ok(session);
        }

        [HttpPost("resume")]
        public async Task<IActionResult> PauseSession([FromHeader(Name = Headers.UID)] Guid userId, [FromBody] ResumeSessionRequest dto) {
            var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.Id == dto.SessionId && s.UserId == userId);
            if (session == null)
            {
                return NotFound("Session not found");
            }

            session.ResumeCounter += 1;
            await _db.SaveChangesAsync();
            return Ok(SFResponse.Ok);
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndSession([FromHeader(Name = Headers.UID)] Guid userId, [FromBody] EndSessionRequest dto) {
            var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.Id == dto.SessionId && s.UserId == userId);
            if (session == null)
            {
                return NotFound("Session not found");
            }

            session.EndTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(SFResponse.Ok);
        }
    }
}