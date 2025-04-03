using System.ComponentModel.DataAnnotations;
using SFServer.Shared.Models.UserProfile;

namespace SFServer.UI.Models.UserProfiles;

public class EditUserProfileViewModel
{
    public Guid Id { get; set; }
    [Required]
    public string Username { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    public UserRole Role { get; set; }
    
    public string? GoogleId { get; set; }
    
    public string? AppleId { get; set; }
    
    public string? FacebookId { get; set; }

    // New properties for password change
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
    
    
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }
    
    public List<WalletItemViewModel> WalletItems { get; set; } = [];
    
    public bool DebugMode { get; set; }
}