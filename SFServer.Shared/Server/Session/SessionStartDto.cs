using System;
using MemoryPack;

namespace SFServer.Shared.Server.Session
{
    [MemoryPackable]
    public partial class SessionStartDto
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
    }
}
