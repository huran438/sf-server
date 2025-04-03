using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Models.UserProfile;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class UserProfilesController : ControllerBase
    {
        private readonly UserProfilesDbContext _context;

        public UserProfilesController(UserProfilesDbContext context)
        {
            _context = context;
        }

        // GET: api/UserProfiles
        [HttpGet]
        public async Task<IActionResult> GetUserProfiles()
        {
            var profiles = await _context.UserProfiles.ToListAsync();
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
            if (await _context.UserProfiles.AnyAsync(u => u.Username.ToLower() == profile.Username.ToLower()))
            {
                return BadRequest("Username already exists.");
            }
    
            // Check if email exists (case-insensitive)
            if (await _context.UserProfiles.AnyAsync(u => u.Email.ToLower() == profile.Email.ToLower()))
            {
                return BadRequest("Email already exists.");
            }
    
            profile.CreatedAt = DateTime.UtcNow;
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
    
            // Optionally, return the newly created user using GetById endpoint.
            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await _context.UserProfiles.FindAsync(id);
            if (profile == null)
                return NotFound();

            if (profile.Role == UserRole.Admin)
                return Forbid("Cannot delete admin users.");

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // GET: api/UserProfiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _context.UserProfiles.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UserProfile updated)
        {
            if (id != updated.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var existing = await _context.UserProfiles.FindAsync(id);
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
                await _context.SaveChangesAsync();
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