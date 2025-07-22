using System;
using MemoryPack;

namespace SFServer.Shared.Server.Purchases
{
    [MemoryPackable]
    public partial class Product
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ProductType Type { get; set; }
    }
}
