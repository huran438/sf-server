

using System;

namespace SFServer.Shared.Models.Auth
{
    public class GooglePlayLoginRequest : ISFServerModel
    {
        public string Credential { get; set; }
        public string GoogleClientId { get; set; }
        public string Token { get; set; }
        
    }
}