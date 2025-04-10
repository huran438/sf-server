using MessagePack;
using SFServer.Shared.Client.Base;
using SFServer.Shared.Client.Common;

namespace SFServer.Shared.Client.Auth
{
    
    [Union(0, typeof(LoginRequest))]
    [Union(1, typeof(GooglePlayLoginRequest))]
    public abstract class LoginRequestBase : SFRequest
    {
        [Key(0)]
        public string Credential { get; set; }
        
        [Key(1)]
        public string DeviceId { get; set; }

        [Key(2)]
        public DeviceInfo DeviceInfo { get; set; }

        [Key(3)]
        public ApplicationInfo ApplicationInfo { get; set; }
    }
}