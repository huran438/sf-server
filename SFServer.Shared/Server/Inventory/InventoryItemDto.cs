using System;
using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Server.Inventory
{
    [MemoryPackable]
    public partial class InventoryItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
}
