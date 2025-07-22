using System;
using MemoryPack;
using System.ComponentModel.DataAnnotations;

namespace SFServer.Shared.Server.Purchases
{
    [MemoryPackable]
    public partial class Product
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        [Required]
        [RegularExpression("^[a-z0-9][a-z0-9_.]*$", ErrorMessage = "The product ID must start with a number or lowercase letter, and can also contain underscores (_) and periods (.)")]
        public string ProductId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ProductType Type { get; set; }
    }
}
