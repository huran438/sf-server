using System;

namespace SFServer.Shared.Models.Auth
{
    public class LoginRequest : ISFServerModel
    {
        public string Credential { get; set; }
        public string DeviceId { get; set; }
    }
}