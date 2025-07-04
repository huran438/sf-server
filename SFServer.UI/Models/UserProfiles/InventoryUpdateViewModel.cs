using System;
using System.Collections.Generic;

namespace SFServer.UI.Models.UserProfiles
{
    public class InventoryEntry
    {
        public Guid? ItemId { get; set; }
        public int Amount { get; set; }
    }

    public class InventoryUpdateViewModel
    {
        public Guid UserId { get; set; }
        public List<InventoryEntry> Items { get; set; } = new();
    }
}
