using System;

namespace SFServer.UI.Models.UserProfiles
{
    public class PlayerPurchaseViewModel
    {
        public Guid ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; }
    }
}
