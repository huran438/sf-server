using MemoryPack;

namespace SFServer.Shared.Client.Auth
{
    [MemoryPackable]
    public partial class LoginRequest : LoginRequestBase
    {
        public override string Endpoint => "Auth/login";
    }
}