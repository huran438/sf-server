using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFServer.API.Data;
using SFServer.Shared.Server.Settings;

namespace SFServer.API.Controllers {
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : ControllerBase {
        private readonly DatabseContext _db;

        public ServerSettingsController(DatabseContext db) {
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSettings() {
            var settings = await _db.ServerSettings.FirstOrDefaultAsync();
            if (settings == null)
                return NotFound();
            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] ServerSettings updated) {
            var existing = await _db.ServerSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                updated.Id = Guid.NewGuid();
                _db.ServerSettings.Add(updated);
            }
            else
            {
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