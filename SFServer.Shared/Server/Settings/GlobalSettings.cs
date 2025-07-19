using System;
using MemoryPack;

namespace SFServer.Shared.Server.Settings
{
    [MemoryPackable]
    public partial class GlobalSettings
    {
        public Guid Id { get; set; }
        public string ServerTitle { get; set; } = string.Empty;
        public string ServerCopyright { get; set; } = string.Empty;
    }
}
