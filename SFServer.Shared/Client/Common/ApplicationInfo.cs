using MemoryPack;

namespace SFServer.Shared.Client.Common
{
    [MemoryPackable]
    public partial class ApplicationInfo
    {
        public string AppIdentifier { get; set; }
        public string AppVersion { get; set; }
        public string UnityVersion { get; set; }
        public string Platform { get; set; }
    }
}