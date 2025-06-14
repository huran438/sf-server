using System;
using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Server.Configs
{
    [MemoryPackable]
    public partial class ConfigMetadata
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public ConfigEnvironment Environment { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<ConfigFile> Files { get; set; } = new();
    }
}
