using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Analytics
{
    [MemoryPackable]
    public partial class AnalyticsEventResponse : ISFResponse
    {
        public bool Accepted { get; set; }
    }
}
