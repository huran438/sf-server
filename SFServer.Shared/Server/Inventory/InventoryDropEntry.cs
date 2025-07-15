using System;
using MemoryPack;

namespace SFServer.Shared.Server.Inventory
{
    [MemoryPackable]
    public partial class InventoryDropEntry
    {
        public Guid? ItemId { get; set; }
        public Guid? CurrencyId { get; set; }
        public int Amount { get; set; }
    }
}
