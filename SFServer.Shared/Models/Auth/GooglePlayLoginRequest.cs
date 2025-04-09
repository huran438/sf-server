using System;
using MessagePack;

namespace SFServer.Shared.Models.Auth
{
    [MessagePackObject]
    public class GooglePlayLoginRequest : LoginRequestBase
    {
        [Key(4)]
        public string GoogleClientId { get; set; }
        [Key(5)]
        public string AuthCode { get; set; }
    }
}