using MemoryPack;
using System.Collections.Generic;
using SFServer.Shared.Server.Configs;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class GetConfigResponse
    {
        public ConfigMetadata Metadata { get; set; }
        public List<ConfigFileContent> Files { get; set; } = new();
    }
}
