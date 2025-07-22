using System;
using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Server.Inventory
{
    [MemoryPackable]
    public partial class InventoryItem
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; }
        public InventoryItemType Type { get; set; }
        public InventoryItemRarity Rarity { get; set; }
        public decimal Price { get; set; }
        /// <summary>
        /// Optional product id used for Google Play or App Store purchases.
        /// </summary>
        public string ProductId { get; set; }
        public bool IsAvailableToBuy { get; set; }
        public bool IsAvailableToDrop { get; set; }
        public List<string> Tags { get; set; } = new();
        /// <summary>
        /// Rewards that will be granted when this item is unpacked.
        /// </summary>
        public List<InventoryDropEntry> Drop { get; set; } = new();

        /// <summary>
        /// If true the item will be automatically unpacked when added to a player's inventory.
        /// </summary>
        public bool AutoUnpack { get; set; }
    }
}
