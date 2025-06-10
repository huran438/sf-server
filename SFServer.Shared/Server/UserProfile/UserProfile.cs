using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MemoryPack;

namespace SFServer.Shared.Server.UserProfile
{
    [MemoryPackable]
    public partial class UserProfile
    {
        public Guid Id { get; set; }
        public int Index { get; private set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public int? Age { get; set; }
        
        [CanBeNull]
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEditAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public UserRole Role { get; set; }
        public string PasswordHash { get; set; }
        public string GooglePlayId { get; set; }
        public bool DebugMode { get; set; }
        public List<string> DeviceIds { get; set; } = new();
    }
}