using MessagePack;
using SFServer.Shared.Models.Base;

namespace SFServer.Shared.Models.Auth
{
    [MessagePackObject]
    public class LoginDashboardRequest : ISFServerModel
    {
        [Key(0)]
        public string Credential { get; set; }

        [Key(1)]
        public string Password { get; set; }
    }
}