using System;
using MessagePack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Auth
{
    [MessagePackObject]
    public class LoginResponse : ISFResponse
    {
        [Key(0)]
        public Guid UserId { get; set; }

        [Key(1)]
        public string Username { get; set; }

        [Key(2)]
        public string Email { get; set; }
        
        [Key(3)]
        public DateTime ExpirationDate { get; set; }

        [Key(4)]
        public string JwtToken { get; set; }
    }
}