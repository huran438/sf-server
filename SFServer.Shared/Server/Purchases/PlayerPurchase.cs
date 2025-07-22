using System;
using MemoryPack;

namespace SFServer.Shared.Server.Purchases
{
    [MemoryPackable]
    public partial class PlayerPurchase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime PurchasedAt { get; set; }
        public Product Product { get; set; }
    }
}
