using System;
using MemoryPack;

namespace SFServer.Shared.Server.Inventory;

[MemoryPackable]
public partial class PlayerInventoryDto
{
    public Guid ItemId { get; set; }
    public int Amount { get; set; }
}
