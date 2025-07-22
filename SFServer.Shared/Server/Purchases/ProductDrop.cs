using System;
using MemoryPack;

namespace SFServer.Shared.Server.Purchases
{
    [MemoryPackable]
    public partial class ProductDrop
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public ProductDropType Type { get; set; }
        public Guid TargetId { get; set; }
        public decimal Amount { get; set; }
    }
}
