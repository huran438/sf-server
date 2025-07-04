using System;
using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Client.Analytics
{
    [MemoryPackable]
    public partial class AnalyticsEventDto
    {
        public string Id { get; set; } = default!;
        public Dictionary<string, string> Params { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
