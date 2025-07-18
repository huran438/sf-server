using System;
using MemoryPack;

namespace SFServer.Shared.Server.Admin;

[MemoryPackable]
public partial class Administrator
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
