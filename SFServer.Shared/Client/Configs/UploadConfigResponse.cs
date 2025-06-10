using MemoryPack;
using SFServer.Shared.Server.Configs;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class UploadConfigResponse
    {
        public ConfigMetadata Metadata { get; set; }
    }
}
