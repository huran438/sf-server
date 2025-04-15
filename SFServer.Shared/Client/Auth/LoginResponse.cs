using System;
using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Auth
{
    [MemoryPackable]
    public partial class LoginResponse : ISFResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string JwtToken { get; set; }
    }
}