using System;
using MemoryPack;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.Shared.Server.Auth
{
    [MemoryPackable]
    public partial class DashboardLoginResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string JwtToken { get; set; }
    }
}