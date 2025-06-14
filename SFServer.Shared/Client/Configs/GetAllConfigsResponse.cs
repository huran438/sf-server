using System.Collections.Generic;
using MemoryPack;
using SFServer.Shared.Server.Configs;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class GetAllConfigsResponse
    {
        public List<ConfigMetadata> Configs { get; set; }
    }
}
