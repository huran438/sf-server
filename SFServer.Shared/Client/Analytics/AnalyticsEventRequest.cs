using System.Collections.Generic;
using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Analytics
{
    [MemoryPackable]
    public partial class AnalyticsEventRequest : SFRequest
    {
        public List<AnalyticsEventDto> Events { get; set; } = new();
        public override string Endpoint => "Analytics/batch";
    }
}
