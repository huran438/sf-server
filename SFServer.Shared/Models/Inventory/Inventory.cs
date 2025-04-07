using System;
using System.Collections.Generic;

namespace SFServer.Shared.Models.Inventory
{
    public class Inventory
    {
        public Guid Id { get; set; } // Inventory ID
        public Guid UserId { get; set; } // The ID of the user who owns this inventory
        public UserProfile.UserProfile User { get; set; } // Navigation property to User
        public List<InventoryItem> Items { get; set; } // List of items in the inventory

        public Inventory()
        {
            Items = new List<InventoryItem>();
        }
    }


}