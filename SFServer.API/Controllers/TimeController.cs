using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class TimeController : ControllerBase
{
    [HttpGet("now")]
    public IActionResult GetServerTime()
    {
        var now = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        return Ok(now);
    }
}