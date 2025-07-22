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
    public class ProjectSettingsController : ControllerBase {
        private readonly DatabseContext _db;

        public ProjectSettingsController(DatabseContext db) {
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSettings(Guid projectId) {
            var settings = await _db.ProjectSettings.FirstOrDefaultAsync(s => s.ProjectId == projectId);
            if (settings == null)
                return NotFound();
            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings(Guid projectId, [FromBody] ProjectSettings updated) {
            var existing = await _db.ProjectSettings.FirstOrDefaultAsync(s => s.ProjectId == projectId);
            if (existing == null)
            {
                updated.Id = Guid.CreateVersion7();
                updated.ProjectId = projectId;
                if (updated.BundleId == null)
                    updated.BundleId = string.Empty;
                _db.ProjectSettings.Add(updated);
            }
            else
            {
                existing.ServerTitle = updated.ServerTitle;
                existing.ServerCopyright = updated.ServerCopyright;
                existing.GoogleClientId = updated.GoogleClientId;
                existing.GoogleClientSecret = updated.GoogleClientSecret;
                existing.ClickHouseConnection = updated.ClickHouseConnection;
                existing.BundleId = updated.BundleId;
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