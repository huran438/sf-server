using MemoryPack;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class DeleteConfigResponse
    {
        public bool Success { get; set; }
    }
}
