using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Purchases
{
    [MemoryPackable]
    public partial class AndroidPurchaseValidationRequest : SFRequest
    {
        public string PackageName { get; set; }
        public string ProductId { get; set; }
        public string PurchaseToken { get; set; }

        public override string Endpoint => "Purchases/validate-android";
    }
}