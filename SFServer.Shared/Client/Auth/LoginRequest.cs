using MessagePack;

namespace SFServer.Shared.Client.Auth
{
    [MessagePackObject]
    public class LoginRequest : LoginRequestBase
    {
        [IgnoreMember]
        public override string Endpoint => "Auth/login";
    }
}