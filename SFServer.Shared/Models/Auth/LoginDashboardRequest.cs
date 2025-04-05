using System;

namespace SFServer.Shared.Models.Auth
{
    public class LoginDashboardRequest : ISFServerModel
    {
        public string Credential { get; set; }
        
        public string Password { get; set; }
    }
}