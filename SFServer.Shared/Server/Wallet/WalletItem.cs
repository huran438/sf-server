using System;

namespace SFServer.Shared.Server.Wallet
{
    public class WalletItem
    {
        public Guid Id { get; set; }

        // Foreign key to user
        public Guid UserId { get; set; }

        // Foreign key to the Currency definition
        public Guid CurrencyId { get; set; }

        // Current amount in wallet.
        public decimal Amount { get; set; }

        // Navigation property
        public Currency Currency { get; set; }
    }
}