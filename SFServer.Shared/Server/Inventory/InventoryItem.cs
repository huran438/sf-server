using System;
using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Server.Inventory;

[MemoryPackable]
public partial class InventoryItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public InventoryItemType Type { get; set; }
    public InventoryItemRarity Rarity { get; set; }
    // Mapping of currency ID to price amount
    public Dictionary<Guid, decimal> Prices { get; set; } = new();
    public bool IsAvailableToBuy { get; set; }
    public bool IsAvailableToDrop { get; set; }
    public List<string> Tags { get; set; } = new();
}
