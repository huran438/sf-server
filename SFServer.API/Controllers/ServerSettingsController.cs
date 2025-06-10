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

        [HttpGet("s3")]
        public async Task<ActionResult<S3SettingsDto>> GetS3()
        {
            var entity = await _db.S3Settings.FirstOrDefaultAsync();
            var dto = new S3SettingsDto
            {
                Bucket = entity?.Bucket ?? _config["S3:Bucket"] ?? string.Empty,
                AccessKeyId = entity?.AccessKeyId ?? _config["AWS_ACCESS_KEY_ID"] ?? string.Empty,
                SecretAccessKey = entity?.SecretAccessKey ?? _config["AWS_SECRET_ACCESS_KEY"] ?? string.Empty,
                Region = entity?.Region ?? _config["AWS_REGION"] ?? string.Empty
            };
            return Ok(dto);
        }

        [HttpPost("s3")]
        public async Task<IActionResult> UpdateS3([FromBody] S3SettingsDto dto)
        {
            var entity = await _db.S3Settings.FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new S3Settings { Id = Guid.NewGuid() };
                _db.S3Settings.Add(entity);
            }

            entity.Bucket = dto.Bucket;
            entity.AccessKeyId = dto.AccessKeyId;
            entity.SecretAccessKey = dto.SecretAccessKey;
            entity.Region = dto.Region;
            await _db.SaveChangesAsync();

            Environment.SetEnvironmentVariable("S3__Bucket", dto.Bucket);
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", dto.AccessKeyId);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", dto.SecretAccessKey);
            Environment.SetEnvironmentVariable("AWS_REGION", dto.Region);
            return NoContent();
        }
    }
}
