using System;

namespace SFServer.UI.Models.UserProfiles
{
    public class PlayerInventoryItemViewModel
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Amount { get; set; }
    }
}
