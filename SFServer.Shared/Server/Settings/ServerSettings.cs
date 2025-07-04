using System;
using MemoryPack;

namespace SFServer.Shared.Server.Settings
{
    [MemoryPackable]
    public partial class ServerSettings
    {
        public Guid Id { get; set; }
        public string ServerCopyright { get; set; } = string.Empty;
        public string GoogleClientId { get; set; } = string.Empty;
        public string ClickHouseConnection { get; set; } = string.Empty;
        public string GoogleClientSecret { get; set; } = string.Empty;
        public string GoogleServiceAccountJson { get; set; } = string.Empty;
    }
}
