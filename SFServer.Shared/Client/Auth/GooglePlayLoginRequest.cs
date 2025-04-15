using MemoryPack;


namespace SFServer.Shared.Client.Auth
{
    [MemoryPackable]
    public partial class GooglePlayLoginRequest : LoginRequestBase
    {
        public string GoogleClientId { get; set; }
        public string AuthCode { get; set; }
        public override string Endpoint => "Auth/GooglePlayLogin";
    }
}