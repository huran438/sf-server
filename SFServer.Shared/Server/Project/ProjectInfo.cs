using System;
using MemoryPack;

namespace SFServer.Shared.Server.Project;

[MemoryPackable]
public partial class ProjectInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
