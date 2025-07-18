using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFServer.API.Data;
using SFServer.API;
using SFServer.Shared.Server.Settings;

namespace SFServer.API.Controllers {
    [ApiController]
    [Route("{projectId:guid}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : ControllerBase {
        private readonly DatabseContext _db;

        public ServerSettingsController(DatabseContext db) {
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSettings(Guid projectId) {
            var settings = await _db.ServerSettings.FirstOrDefaultAsync(s => s.ProjectId == projectId);
            if (settings == null)
                return NotFound();
            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings(Guid projectId, [FromBody] ServerSettings updated) {
            var existing = await _db.ServerSettings.FirstOrDefaultAsync(s => s.ProjectId == projectId);
            if (existing == null)
            {
                updated.Id = Guid.NewGuid();
                updated.ProjectId = projectId;
                _db.ServerSettings.Add(updated);
            }
            else
            {
                existing.ServerTitle = updated.ServerTitle;
                existing.ServerCopyright = updated.ServerCopyright;
                existing.GoogleClientId = updated.GoogleClientId;
                existing.GoogleClientSecret = updated.GoogleClientSecret;
                existing.ClickHouseConnection = updated.ClickHouseConnection;
                if (string.IsNullOrEmpty(updated.GoogleServiceAccountJson) == false) {
                    dynamic parsedJson = JsonConvert.DeserializeObject(updated.GoogleServiceAccountJson);
                    existing.GoogleServiceAccountJson = parsedJson == null ? updated.GoogleServiceAccountJson : (string)JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}