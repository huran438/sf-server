using System;

namespace SFServer.Shared.Models.Auth
{
    public class LoginDashboardRequest
    {
        public string Credential { get; set; }
        
        public string Password { get; set; }
    }
}