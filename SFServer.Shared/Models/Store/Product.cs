using System;

namespace SFServer.Shared.Models.Store
{
    public class Product
    {
        public Guid Id { get; set; }
        public string NodeId { get; set; }
        public ProductType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PayoutType PayoutType { get; set; }
        public string PayoutId { get; set; }
        public string PayoutSubtype { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}