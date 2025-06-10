using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemoryPack;
using SFServer.API.Data;
using SFServer.API.Services;
using SFServer.Shared.Server.Configs;
using SFServer.Shared.Client.Configs;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ConfigsController : ControllerBase
    {
        private readonly DatabseContext _db;
        private readonly S3Service _s3;

        public ConfigsController(DatabseContext db, S3Service s3)
        {
            _db = db;
            _s3 = s3;
        }

        [HttpPost]
        public async Task<ActionResult<ConfigMetadata>> Upload([FromBody] UploadConfigDto dto)
        {
            var meta = new ConfigMetadata
            {
                Id = Guid.NewGuid(),
                Version = dto.Version,
                Environment = dto.Environment,
                UploadedAt = DateTime.UtcNow,
                S3Key = $"configs/{dto.Version}/{dto.Environment}.json"
            };

            await _s3.UploadJsonAsync(meta.S3Key, dto.Config);

            _db.Configs.Add(meta);
            await _db.SaveChangesAsync();
            return Ok(meta);
        }

        [HttpGet("{version}/{environment}")]
        [AllowAnonymous]
        public async Task<ActionResult<GetConfigResponse>> Get(string version, ConfigEnvironment environment)
        {
            var meta = await _db.Configs.FirstOrDefaultAsync(c => c.Version == version && c.Environment == environment);
            if (meta == null)
                return NotFound();

            var json = await _s3.DownloadJsonAsync(meta.S3Key);
            var configBytes = MemoryPackSerializer.Serialize(json);
            return Ok(new GetConfigResponse { Metadata = meta, Config = configBytes });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<GetAllConfigsResponse>> GetAll()
        {
            var configs = new List<GetConfigResponse>();
            var metas = await _db.Configs.ToListAsync();
            foreach (var meta in metas)
            {
                var json = await _s3.DownloadJsonAsync(meta.S3Key);
                var configBytes = MemoryPackSerializer.Serialize(json);
                configs.Add(new GetConfigResponse
                {
                    Metadata = meta,
                    Config = configBytes
                });
            }

            return Ok(new GetAllConfigsResponse { Configs = configs });
        }

        [HttpDelete("{version}/{environment}")]
        public async Task<IActionResult> Delete(string version, ConfigEnvironment environment)
        {
            var meta = await _db.Configs.FirstOrDefaultAsync(c => c.Version == version && c.Environment == environment);
            if (meta == null)
                return NotFound();

            await _s3.DeleteAsync(meta.S3Key);
            _db.Configs.Remove(meta);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    [MemoryPackable]
    public partial class UploadConfigDto
    {
        public string Version { get; set; }
        public ConfigEnvironment Environment { get; set; }
        public string Config { get; set; }
    }

}
