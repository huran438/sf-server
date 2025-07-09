using System;
using MemoryPack;

namespace SFServer.Shared.Server.Audit
{
    [MemoryPackable]
    public partial class AuditLogEntry
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
