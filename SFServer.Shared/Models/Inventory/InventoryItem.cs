using System;

namespace SFServer.Shared.Models.Inventory
{
    public class InventoryItem
    {
        public Guid Id { get; set; } // Changed to Guid
        public string Title { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; } // e.g., Weapon, Consumable, Armor
        public string ImageUrl { get; set; } // URL to the item's image
        public DateTime CreatedAt { get; set; }
    }

}