using System;
using MemoryPack;

namespace SFServer.Shared.Server.Wallet
{
    [MemoryPackable]
    public partial class WalletUpdateDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
    }
}