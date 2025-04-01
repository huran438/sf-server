namespace SecretGameBackend.Shared.Models.Wallet
{
    public class WalletUpdateDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
    }
}