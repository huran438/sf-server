

using System;

namespace SFServer.Shared.Models.Auth
{
    public class GooglePlayLoginRequest
    {
        public string Credential { get; set; }
        public string Token { get; set; }
    }
}