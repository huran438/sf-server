namespace SFServer.Shared.Models.Auth
{
    public class LoginRequest
    {
        public string Credential { get; set; }
        
        public string Password { get; set; }
        
        public bool AdminPanel { get; set; }
    }
}