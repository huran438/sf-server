using System;
using MemoryPack;

namespace SFServer.Shared.Server.Wallet
{
    [MemoryPackable]
    public partial class Currency
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        public string Title { get; set; }
        
        public string Icon { get; set; }
        
        public string RichText { get; set; }
        
        public int InitialAmount { get; set; }
        
        public int Capacity { get; set; }
        
        public int RefillSeconds { get; set; }

        public string Color { get; set; }
    }
}