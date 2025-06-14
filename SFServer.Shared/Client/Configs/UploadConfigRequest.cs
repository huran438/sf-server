using MemoryPack;
using SFServer.Shared.Client.Base;
using SFServer.Shared.Server.Configs;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class UploadConfigRequest : SFRequest
    {
        public string Version { get; set; }
        public ConfigEnvironment Environment { get; set; }
        // JSON payload for the configuration. Stored as raw string to avoid
        // serialization issues with MemoryPack.
        public string Config { get; set; }
        public override string Endpoint => "Configs";
    }
}
