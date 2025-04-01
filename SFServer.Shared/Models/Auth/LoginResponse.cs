namespace SecretGameBackend.Shared.Models.Auth;

public class LoginResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string JwtToken { get; set; }
}