using System;

namespace SFServer.UI.Models.UserProfiles
{
    public class WalletItemViewModel
    {
        public Guid Id { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
}