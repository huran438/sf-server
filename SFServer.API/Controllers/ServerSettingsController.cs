using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemoryPack;
using SFServer.Shared.Client.ServerSettings;
using SFServer.Shared.Server.Settings;
using SFServer.API.Data;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DatabseContext _db;

        public ServerSettingsController(IConfiguration config, DatabseContext db)
        {
            _config = config;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ServerSettingsDto>> Get()
        {
            var entity = await _db.ServerSettings.FirstOrDefaultAsync();
            var dto = new ServerSettingsDto
            {
                Bucket = entity?.Bucket ?? _config["S3:Bucket"] ?? string.Empty,
                AccessKeyId = entity?.AccessKeyId ?? _config["AWS_ACCESS_KEY_ID"] ?? string.Empty,
                SecretAccessKey = entity?.SecretAccessKey ?? _config["AWS_SECRET_ACCESS_KEY"] ?? string.Empty,
                Region = entity?.Region ?? _config["AWS_REGION"] ?? string.Empty,
                Url = entity?.Url ?? _config["S3:Url"] ?? string.Empty,
                GoogleClientId = entity?.GoogleClientId ?? _config["GOOGLE_CLIENT_ID"] ?? string.Empty,
                GoogleClientSecret = entity?.GoogleClientSecret ?? _config["GOOGLE_CLIENT_SECRET"] ?? string.Empty
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ServerSettingsDto dto)
        {
            var entity = await _db.ServerSettings.FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new ServerSettings { Id = Guid.NewGuid() };
                _db.ServerSettings.Add(entity);
            }

            entity.Bucket = dto.Bucket;
            entity.AccessKeyId = dto.AccessKeyId;
            entity.SecretAccessKey = dto.SecretAccessKey;
            entity.Region = dto.Region;
            entity.Url = dto.Url;
            entity.GoogleClientId = dto.GoogleClientId;
            entity.GoogleClientSecret = dto.GoogleClientSecret;
            await _db.SaveChangesAsync();

            Environment.SetEnvironmentVariable("S3__Bucket", dto.Bucket);
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", dto.AccessKeyId);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", dto.SecretAccessKey);
            Environment.SetEnvironmentVariable("AWS_REGION", dto.Region);
            Environment.SetEnvironmentVariable("S3__Url", dto.Url);
            Environment.SetEnvironmentVariable("GOOGLE_CLIENT_ID", dto.GoogleClientId);
            Environment.SetEnvironmentVariable("GOOGLE_CLIENT_SECRET", dto.GoogleClientSecret);
            return NoContent();
        }
    }
}
