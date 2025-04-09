
using MessagePack;

namespace SFServer.Shared.Models.Common
{
    [MessagePackObject]
    public class ApplicationInfo
    {
        [Key(0)]
        public string appIdentifier;
        [Key(1)]
        public string appVersion;
        [Key(2)]
        public string unityVersion;
        [Key(3)]
        public string platform;
    }
}