using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class GetAllConfigsResponse
    {
        public List<GetConfigResponse> Configs { get; set; }
    }
}
