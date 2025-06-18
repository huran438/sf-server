using System;
using MemoryPack;

namespace SFServer.Shared.Server.Session
{
    [MemoryPackable]
    public partial class UserSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsPaused { get; set; }
    }
}
