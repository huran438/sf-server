using System;
using MemoryPack;
using SFServer.Shared.Server.Purchases;

namespace SFServer.Shared.Server.Inventory
{
    [MemoryPackable]
    public partial class InventoryItemDrop
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public ProductDropType Type { get; set; }
        public Guid TargetId { get; set; }
        public int Amount { get; set; }
    }
}
