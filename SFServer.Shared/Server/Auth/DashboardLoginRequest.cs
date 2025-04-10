using MessagePack;

namespace SFServer.Shared.Server.Auth
{
    [MessagePackObject]
    public class LoginDashboardRequest
    {
        [Key(0)]
        public string Credential { get; set; }

        [Key(1)]
        public string Password { get; set; }
    }
}