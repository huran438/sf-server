using SFServer.Shared.Server.Configs;

namespace SFServer.UI.Models
{
    public class ConfigsViewModel
    {
        public List<ConfigMetadata> Configs { get; set; } = new();
        public string Version { get; set; } = string.Empty;
        public ConfigEnvironment Environment { get; set; }
        public string ConfigJson { get; set; } = string.Empty;
    }
}
