
using System;
using SFServer.Shared.Models.UserProfile;

namespace SFServer.Shared.Models.Auth
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string JwtToken { get; set; }
        public bool DebugMode { get; set; }
    }
}