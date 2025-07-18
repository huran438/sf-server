using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly DatabseContext _db;
        public AuditLogController(DatabseContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuditLogEntry>>> Get([FromQuery] int count = 100)
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var logs = await _db.AuditLogs
                .Where(l => l.ProjectId == projectId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
            return Ok(logs);
        }

        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var toRemove = _db.AuditLogs.Where(l => l.ProjectId == projectId);
            _db.AuditLogs.RemoveRange(toRemove);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var logs = await _db.AuditLogs
                .Where(l => l.ProjectId == projectId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            var csvLines = logs.Select(l => $"{l.Timestamp:O},{l.UserId},{l.Method},{l.Path},{l.StatusCode}");
            var csv = string.Join("\n", csvLines);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"auditlog_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}
