using MemoryPack;

namespace SFServer.Shared.Server.Google
{
    [MemoryPackable]
    public partial class GooglePlayerInfo
    {
        public string Kind { get; set; }
        public string PlayerId { get; set; }
        public string DisplayName { get; set; }
        public string AvatarImageUrl { get; set; }
        public string Title { get; set; }
        public PlayerName Name { get; set; }
    }
}