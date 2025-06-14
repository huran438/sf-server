using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemoryPack;
using SFServer.API.Data;
using SFServer.API.Services;
using SFServer.Shared.Server.Configs;
using SFServer.Shared.Client.Configs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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
        [RequestSizeLimit(long.MaxValue)]
        public async Task<ActionResult<ConfigMetadata>> Upload([FromForm] UploadConfigDto dto)
        {
            var meta = new ConfigMetadata
            {
                Id = Guid.NewGuid(),
                Version = dto.Version,
                Environment = dto.Environment,
                UploadedAt = DateTime.UtcNow,
            };

            foreach (var file in dto.Files)
            {
                var key = $"configs/{meta.Version}/{meta.Environment}/{meta.UploadedAt:yyyyMMddHHmmss}/{file.FileName}";
                await _s3.UploadStreamAsync(key, file.OpenReadStream(), file.ContentType);
                meta.Files.Add(new ConfigFile
                {
                    Id = Guid.NewGuid(),
                    FileName = file.FileName,
                    S3Key = key
                });
            }

            _db.Configs.Add(meta);
            await _db.SaveChangesAsync();
            return Ok(meta);
        }

        [HttpGet("{version}/{environment}")]
        [AllowAnonymous]
        public async Task<ActionResult<GetConfigResponse>> Get(string version, ConfigEnvironment environment)
        {
            var meta = await _db.Configs
                .Include(c => c.Files)
                .Where(c => c.Version == version && c.Environment == environment)
                .OrderByDescending(c => c.UploadedAt)
                .FirstOrDefaultAsync();
            if (meta == null)
                return NotFound();

            var files = new List<ConfigFileContent>();
            foreach (var file in meta.Files)
            {
                var json = await _s3.DownloadJsonAsync(file.S3Key);
                files.Add(new ConfigFileContent
                {
                    FileName = file.FileName,
                    Config = MemoryPackSerializer.Serialize(json)
                });
            }

            return Ok(new GetConfigResponse { Metadata = meta, Files = files });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<GetAllConfigsResponse>> GetAll()
        {
            var metas = await _db.Configs.Include(c => c.Files).ToListAsync();
            return Ok(new GetAllConfigsResponse { Configs = metas });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var meta = await _db.Configs.Include(c => c.Files).FirstOrDefaultAsync(c => c.Id == id);
            if (meta == null)
                return NotFound();

            foreach (var file in meta.Files)
            {
                await _s3.DeleteAsync(file.S3Key);
            }

            _db.ConfigFiles.RemoveRange(meta.Files);
            _db.Configs.Remove(meta);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public class UploadConfigDto
    {
        public string Version { get; set; } = string.Empty;
        public ConfigEnvironment Environment { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }

}
