using MemoryPack;

namespace SFServer.Shared.Client.Base
{
    public abstract partial class SFRequest : ISFRequest
    {
        [MemoryPackIgnore]
        public abstract string Endpoint { get; }
    }
}