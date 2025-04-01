namespace SecretGameBackend.Shared.Models.UserProfile;

public class UserProfile
{
    public Guid Id { get; set; }
    public string Username { get; set; }

    public string? FullName { get; set; }

    public int? Age { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastEditAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    
    public UserRole Role { get; set; }
    public string PasswordHash { get; set; }
    public string? GooglePlayId { get; set; }
}