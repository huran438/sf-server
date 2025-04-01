using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SFServer.API.Data;
using SecretGameBackend.Shared.Models.Auth;
using SecretGameBackend.Shared.Models.UserProfile;
using shortid;
using shortid.Configuration;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserProfilesDbContext _db;

    public AuthController(IConfiguration config, UserProfilesDbContext db)
    {
        _config = config;
        _db = db;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request.Credential))
        {
            return await AnonymousLogin();
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

        // Only allow Admin or Developer logins (or allow others as per your business logic)
        if (request.AdminPanel && user.Role != UserRole.Admin && user.Role != UserRole.Developer)
            return Unauthorized("You do not have permission to access this service.");

        if (user.Role != UserRole.Guest)
        {
            var hasher = new PasswordHasher<UserProfile>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid password");
        }

        // Create JWT token.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
        var expirationDate = GetExpirationDate();
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expirationDate,
            signingCredentials: creds
        );
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpirationDate = expirationDate,
            JwtToken = jwtToken
        };

        return Ok(response);
    }

    private DateTime GetExpirationDate()
    {
        return DateTime.UtcNow.AddHours(int.TryParse(_config["JWT_TIMEOUT_HOURS"], out var hours) ? hours : 1);
    }

    private string GenerateUsername()
    {
        return $"Guest{ShortId.Generate(new GenerationOptions(length: 8, useSpecialCharacters: false, useNumbers: true)).ToUpper()}";
    }

    private async Task<IActionResult> AnonymousLogin()
    {
        var user = new UserProfile
        {
            Username = GenerateUsername(),
            Email = string.Empty, // Email not required
            Role = UserRole.Guest, // Regular user role
            CreatedAt = DateTime.UtcNow,
            LastEditAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        // Generate a dummy password hash (since password isn't used in anonymous login)
        var hasher = new PasswordHasher<UserProfile>();
        user.PasswordHash = hasher.HashPassword(user, Guid.NewGuid().ToString());

        _db.UserProfiles.Add(user);
        await _db.SaveChangesAsync();

        // Generate JWT token
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expirationDate = GetExpirationDate();
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expirationDate,
            signingCredentials: creds
        );
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpirationDate = expirationDate,
            JwtToken = jwtToken
        };

        return Ok(response);
    }

    // POST /Auth/GooglePlayLogin
    [HttpPost("GooglePlayLogin")]
    [AllowAnonymous]
    public async Task<IActionResult> GooglePlayLogin([FromBody] GooglePlayLoginRequest request)
    {
        // Validate token using Google's API
        GoogleJsonWebSignature.Payload payload;
        try
        {
            // VerifyAsync checks the token signature and expiration.
            // Make sure to set your expected audience (client ID) in the validation settings.
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _config["GOOGLE_CLIENT_ID"] }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);
        }
        catch (Exception ex)
        {
            return BadRequest("Invalid Google Play token: " + ex.Message);
        }

        // Extract Google Play account info
        string googlePlayId = payload.Subject; // The unique identifier for the Google account.
        string email = payload.Email;
        string name = payload.Name;

        // Check if a user already exists with this Google Play ID or email.
        var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.GooglePlayId == googlePlayId || (u.Email != null && u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase)));
        if (user == null)
        {
            // Option 1: Create a new user profile for anonymous players.
            user = new UserProfile
            {
                Id = Guid.NewGuid(),
                Username = GenerateUsername(),
                FullName = string.IsNullOrWhiteSpace(name) ? null : name,
                Email = email,
                Role = UserRole.User, // Default role for Google Play users.
                CreatedAt = DateTime.UtcNow,
                LastEditAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                GooglePlayId = googlePlayId
            };
            
            var hasher = new PasswordHasher<UserProfile>();
            user.PasswordHash = hasher.HashPassword(user, Guid.NewGuid().ToString());

            _db.UserProfiles.Add(user);
            await _db.SaveChangesAsync();
        }
        else
        {
            // If user exists, update LastLoginAt and ensure GooglePlayId is set.
            user.LastLoginAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(user.GooglePlayId))
            {
                user.GooglePlayId = googlePlayId;
                await _db.SaveChangesAsync();
            }
        }

        // Generate JWT token.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString())
        };

        var expirationDate = GetExpirationDate();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expirationDate,
            signingCredentials: creds
        );
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpirationDate = expirationDate,
            JwtToken = jwtToken
        };

        return Ok(response);
    }

    // Optionally, add an endpoint to link an existing profile to a Google Play account.
    // This could be used when a user is already authenticated locally, then chooses "Link Google Play".
    [HttpPost("LinkGooglePlay")]
    [Authorize]
    public async Task<IActionResult> LinkGooglePlay([FromBody] GooglePlayLoginRequest request)
    {
        // Validate token as above.
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _config["GOOGLE_PLAY_CLIENT_ID"] }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);
        }
        catch (Exception ex)
        {
            return BadRequest("Invalid Google Play token: " + ex.Message);
        }

        string googlePlayId = payload.Subject;

        // Get the current authenticated user.
        var currentUserId = Guid.Parse(User.FindFirst("UserId")?.Value);
        var user = await _db.UserProfiles.FindAsync(currentUserId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Link the Google Play account.
        user.GooglePlayId = googlePlayId;

        if (user.Role == UserRole.Guest)
        {
            user.Role = UserRole.User;
        }
        
        await _db.SaveChangesAsync();

        return Ok("Google Play account linked.");
    }
}