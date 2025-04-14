using MessagePack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Connection
{
    [MessagePackObject]
    public class CheckConnectionRequest : SFRequest
    {
        [Key(0)]
        public string Credential { get; set; }
        [Key(1)]
        public string DeviceId { get; set; }
        
        [IgnoreMember]
        public override string Endpoint => "Connection/check";
    }
}