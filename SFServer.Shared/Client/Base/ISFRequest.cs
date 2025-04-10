using MessagePack;

namespace SFServer.Shared.Client.Base
{
    public interface ISFRequest
    {
        [IgnoreMember]
        public string Endpoint { get; }
    }
}