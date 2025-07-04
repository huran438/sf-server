using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Purchases
{
    [MemoryPackable]
    public partial class AndroidPurchaseValidationResponse : ISFResponse
    {
        public string PackageName { get; set; }
        public string ProductId { get; set; }
        public bool IsValid { get; set; }
        public bool Acknowledged { get; set; }
        public int PurchaseState { get; set; }
        public int ConsumptionState { get; set; }
        public long PurchaseTimeMillis { get; set; }
        public string Error { get; set; }
    }
}
