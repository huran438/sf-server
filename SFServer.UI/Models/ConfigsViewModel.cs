using SFServer.Shared.Server.Configs;
using Microsoft.AspNetCore.Http;
using System;

namespace SFServer.UI.Models
{
    public class ConfigsViewModel
    {
        public List<ConfigMetadata> Configs { get; set; } = new();
        public string Version { get; set; } = string.Empty;
        public ConfigEnvironment Environment { get; set; }
        public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
    }
}
