using System;
using SFServer.Shared.Models.Base;

namespace SFServer.Shared.Models.Wallet
{
    public class WalletUpdateDto : ISFServerModel
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
    }
}