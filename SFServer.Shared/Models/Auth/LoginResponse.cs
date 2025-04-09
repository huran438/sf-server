using System;
using MessagePack;
using SFServer.Shared.Models.Base;
using SFServer.Shared.Models.UserProfile;

namespace SFServer.Shared.Models.Auth
{
    [MessagePackObject]
    public class LoginResponse : ISFServerMsgPackResponse
    {
        [Key(0)]
        public Guid UserId { get; set; }

        [Key(1)]
        public string Username { get; set; }

        [Key(2)]
        public string Email { get; set; }

        [Key(3)]
        public UserRole Role { get; set; }

        [Key(4)]
        public DateTime ExpirationDate { get; set; }

        [Key(5)]
        public string JwtToken { get; set; }
    }
}