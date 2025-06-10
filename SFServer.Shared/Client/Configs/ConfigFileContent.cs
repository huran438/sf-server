using MemoryPack;

namespace SFServer.Shared.Client.Configs
{
    [MemoryPackable]
    public partial class ConfigFileContent
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Config { get; set; } = System.Array.Empty<byte>();
    }
}
