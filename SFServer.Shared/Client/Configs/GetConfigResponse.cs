using MemoryPack;
using SFServer.Shared.Server.Configs;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class GetConfigResponse
    {
        public ConfigMetadata Metadata { get; set; }
        // MemoryPack-serialized configuration data
        public byte[] Config { get; set; }
    }
}
