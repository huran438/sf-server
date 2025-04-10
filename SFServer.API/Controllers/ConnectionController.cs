using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Client.Connection;
using SFServer.Shared.Server.Auth;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly UserProfilesDbContext _db;

    public ConnectionController(UserProfilesDbContext db)
    {
        _db = db;
    }
    
    [HttpPost("check")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDashboardRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = new CheckConnectionResponse
        {
            ServerTime = DateTime.UtcNow,
            DebugMode = false,
            
        };
        
        if (string.IsNullOrEmpty(request.Credential))
        {
            return Ok(response);
        }

        UserProfile user = null;

        // Try to interpret the credential as an ID (integer)
        if (Guid.TryParse(request.Credential, out Guid id))
        {
            user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id);
        }

        // If not found and if the credential contains '@', try email lookup.
        if (user == null && request.Credential.Contains("@"))
        {
            user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Credential.ToLower());
        }

        // If still not found, try username lookup.
        if (user == null)
        {
            user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Credential.ToLower());
        }

        if (user == null)
            return Unauthorized("User not found");
        
        response.DebugMode = (user.Role is UserRole.Admin or UserRole.Developer) && user.DebugMode;

        return Ok(response);
    }
}