using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("{projectId:guid}/[controller]")]
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
        public async Task<IActionResult> GetUserProfiles(Guid projectId)
        {
            var profiles = await _db.UserProfiles.Where(p => p.ProjectId == projectId).ToListAsync();
            return Ok(profiles);
        }

        // POST: api/UserProfiles
        [HttpPost]
        public async Task<IActionResult> CreateUserProfile(Guid projectId, [FromBody] UserProfile profile)
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
            profile.ProjectId = projectId;
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();

            // Optionally, return the newly created user using GetById endpoint.
            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
        }


        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid projectId, Guid id)
        {
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id && u.ProjectId == projectId);
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
        public async Task<IActionResult> GetById(Guid projectId, Guid id)
        {
            var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id && u.ProjectId == projectId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET: api/UserProfiles/{userId}/device/{deviceId}
        [HttpGet("{userId:guid}/device/{deviceId}")]
        public async Task<IActionResult> GetDeviceById(Guid projectId, Guid userId, string deviceId)
        {
            var userDevice = await _db.UserDevices.FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId);
            if (userDevice == null)
            {
                Console.WriteLine($"Device {deviceId} for user {userId} not found");
                return NotFound();
            }

            return Ok(userDevice);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUserProfile(Guid projectId, Guid id, [FromBody] UserProfile updated)
        {
            if (id != updated.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var existing = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id && u.ProjectId == projectId);
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
            updated.ProjectId = existing.ProjectId;


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