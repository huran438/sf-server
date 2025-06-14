using System;
using MemoryPack;

namespace SFServer.Shared.Server.Configs
{
    [MemoryPackable]
    public partial class ConfigFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string S3Key { get; set; } = string.Empty;
        public Guid ConfigMetadataId { get; set; }
    }
}
