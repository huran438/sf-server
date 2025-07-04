using System;
using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Session {
    [MemoryPackable]
    public partial class UserSession : ISFResponse {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public uint ResumeCounter { get; set; }
    }
}