using MemoryPack;

namespace SFServer.Shared.Server.Google
{
    [MemoryPackable]
    public partial class PlayerName
    {
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
    }
}