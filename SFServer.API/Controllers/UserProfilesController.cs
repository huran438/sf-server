using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class UserProfilesController : ControllerBase
    {
        private readonly DatabseContext _db;

        public UserProfilesController(DatabseContext db)
        {
            _db = db;
        }

        // GET: api/UserProfiles
        [HttpGet]
        public async Task<IActionResult> GetUserProfiles()
        {
            var profiles = await _db.UserProfiles.ToListAsync();
            return Ok(profiles);
        }

        // POST: api/UserProfiles
        [HttpPost]
        public async Task<IActionResult> CreateUserProfile([FromBody] UserProfile profile)
        {
            if (profile == null)
            {
                return BadRequest("Profile cannot be null.");
            }

            // Check if username exists (case-insensitive)
            if (await _db.UserProfiles.AnyAsync(u => u.Username.ToLower() == profile.Username.ToLower()))
            {
                return BadRequest("Username already exists.");
            }

            // Check if email exists (case-insensitive)
            if (await _db.UserProfiles.AnyAsync(u => profile.Email != null && u.Email.ToLower() == profile.Email.ToLower()))
            {
                return BadRequest("Email already exists.");
            }

            profile.CreatedAt = DateTime.UtcNow;
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();

            // Optionally, return the newly created user using GetById endpoint.
            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
        }


        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await _db.UserProfiles.FindAsync(id);
            if (profile == null)
                return NotFound();

            if (profile.Role == UserRole.Admin)
                return Forbid("Cannot delete admin users.");

            // Delete Devices
            _db.UserDevices.RemoveRange(_db.UserDevices.Where(d => d.UserId == profile.Id));
            
            // Wallet
            _db.WalletItems.RemoveRange(_db.WalletItems.Where(d => d.UserId == profile.Id));
            
            _db.UserProfiles.Remove(profile);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/UserProfiles/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _db.UserProfiles.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET: api/UserProfiles/{userId}/device/{deviceId}
        [HttpGet("{userId:guid}/device/{deviceId}")]
        public async Task<IActionResult> GetDeviceById(Guid userId, string deviceId)
        {
            var userDevice = await _db.UserDevices.FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId);
            if (userDevice == null)
            {
                Console.WriteLine($"User device not found for userId={userId}, deviceId={deviceId}");
                return NotFound();
            }
            return Ok(userDevice);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UserProfile updated)
        {
            if (id != updated.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var existing = await _db.UserProfiles.FindAsync(id);
            if (existing == null)
            {
                return NotFound("User not found.");
            }

            // If the current user is a Developer and the target user is an Admin, forbid the update.
            if (User.IsInRole("Developer") && existing.Role == UserRole.Admin)
            {
                return Forbid("Developers cannot edit Admin accounts.");
            }

            // Prevent changing role of an admin user
            if (existing.Role == UserRole.Admin && updated.Role != UserRole.Admin)
            {
                return BadRequest("Cannot change role of an Admin user.");
            }

            // Log the incoming update
            Console.WriteLine($"Updating User: id={id}, new Username={updated.Username}, new Email={updated.Email}, new Role={updated.Role}");

            // Update fields
            existing.Username = updated.Username;
            existing.Email = updated.Email;
            existing.Role = updated.Role;
            existing.LastEditAt = DateTime.UtcNow;
            existing.DebugMode = updated.DebugMode;

            // Determine if current user is trying to change someone else's password
            bool isSelf = User.FindFirst("UserId")?.Value == existing.Id.ToString();
            bool isAdmin = User.IsInRole("Admin");

            if (!string.IsNullOrEmpty(updated.PasswordHash))
            {
                if (!isAdmin && !isSelf)
                {
                    return Forbid("You can only change your own password.");
                }

                // Only allow Admin or self to update password
                existing.PasswordHash = updated.PasswordHash;
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving changes: " + ex.Message);
                return StatusCode(500, "Error saving changes: " + ex.Message);
            }

            Console.WriteLine("User updated successfully.");
            return NoContent();
        }
    }
}