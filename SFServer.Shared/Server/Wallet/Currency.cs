using System;

namespace SFServer.Shared.Server.Wallet
{
    public class Currency
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        
        public string Icon { get; set; }
        
        public string RichText { get; set; }
        
        public int InitialAmount { get; set; }
        
        public int Capacity { get; set; }
        
        public int RefillSeconds { get; set; }

        public string Color { get; set; }
    }
}