using MemoryPack;
using SFServer.Shared.Client.Base;
using SFServer.Shared.Client.Common;

namespace SFServer.Shared.Client.Auth
{
    
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(LoginRequest))]
    [MemoryPackUnion(1, typeof(GooglePlayLoginRequest))]
    public abstract partial class LoginRequestBase : SFRequest
    {
        public string Credential { get; set; }
        public string DeviceId { get; set; }
        public UserDeviceInfo DeviceInfo { get; set; }
        
        public ApplicationInfo ApplicationInfo { get; set; }
    }
}