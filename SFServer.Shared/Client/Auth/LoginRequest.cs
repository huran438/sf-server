using MessagePack;

namespace SFServer.Shared.Client.Auth
{
    [MessagePackObject]
    public class LoginRequest : LoginRequestBase
    {
        public override string Endpoint => "Auth/login";
    }
}