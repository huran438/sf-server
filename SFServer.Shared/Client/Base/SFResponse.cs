using MemoryPack;

namespace SFServer.Shared.Client.Base
{
    [MemoryPackable]
    public partial class SFResponse : ISFResponse
    {
        public static SFResponse Ok = new();
    }
}