using MessagePack;

namespace SFServer.Shared.Client.Base
{
    public abstract class SFRequest : ISFRequest
    {
        [IgnoreMember]
        public abstract string Endpoint { get; }
    }
}