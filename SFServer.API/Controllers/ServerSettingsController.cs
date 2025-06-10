using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MemoryPack;
using SFServer.Shared.Client.ServerSettings;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ServerSettingsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("s3")]
        public ActionResult<S3SettingsDto> GetS3()
        {
            var dto = new S3SettingsDto
            {
                Bucket = _config["S3:Bucket"] ?? string.Empty,
                AccessKeyId = _config["AWS_ACCESS_KEY_ID"] ?? string.Empty,
                SecretAccessKey = _config["AWS_SECRET_ACCESS_KEY"] ?? string.Empty,
                Region = _config["AWS_REGION"] ?? string.Empty
            };
            return Ok(dto);
        }

        [HttpPost("s3")]
        public IActionResult UpdateS3([FromBody] S3SettingsDto dto)
        {
            Environment.SetEnvironmentVariable("S3__Bucket", dto.Bucket);
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", dto.AccessKeyId);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", dto.SecretAccessKey);
            Environment.SetEnvironmentVariable("AWS_REGION", dto.Region);
            return NoContent();
        }
    }
}
