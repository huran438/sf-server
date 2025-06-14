using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class GetAllConfigsRequest : SFRequest
    {
        public override string Endpoint => "Configs";
    }
}
