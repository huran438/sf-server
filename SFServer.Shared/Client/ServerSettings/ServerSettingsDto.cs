using MemoryPack;

namespace SFServer.Shared.Client.ServerSettings
{
    [MemoryPackable]
    public partial class ServerSettingsDto
    {
        public string Bucket { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string GoogleClientId { get; set; } = string.Empty;
        public string GoogleClientSecret { get; set; } = string.Empty;
    }
}
