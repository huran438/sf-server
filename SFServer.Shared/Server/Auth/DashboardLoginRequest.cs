using MemoryPack;

namespace SFServer.Shared.Server.Auth
{
    [MemoryPackable]
    public partial class LoginDashboardRequest
    {
        public string Credential { get; set; }
        
        public string Password { get; set; }
    }
}