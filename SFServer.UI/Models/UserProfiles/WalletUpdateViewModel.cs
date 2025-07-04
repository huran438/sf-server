using System;
using System.Collections.Generic;

namespace SFServer.UI.Models.UserProfiles
{
    public class WalletItemEntry
    {
        public Guid WalletItemId { get; set; }
        public decimal Amount { get; set; }
    }

    public class WalletUpdateViewModel
    {
        public Guid UserId { get; set; }
        public List<WalletItemEntry> WalletItems { get; set; }
    }
}