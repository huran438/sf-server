using System.ComponentModel.DataAnnotations;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.UI.Models.UserProfiles
{
    public class CreateUserProfileViewModel
    {
        [Required]
        public string Username { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;
    }
}