using System;
using MemoryPack;

namespace SFServer.Shared.Server.Inventory;

[MemoryPackable]
public partial class PlayerInventoryItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }
    public int Amount { get; set; }
}
