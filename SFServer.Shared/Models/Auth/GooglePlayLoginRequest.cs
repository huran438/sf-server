using System;

namespace SFServer.Shared.Models.Auth
{
    public class GooglePlayLoginRequest : ISFServerModel
    {
        public string Credential { get; set; }
        public string DeviceId { get; set; }
        public string GoogleClientId { get; set; }
        public string AuthCode { get; set; }
    }
}