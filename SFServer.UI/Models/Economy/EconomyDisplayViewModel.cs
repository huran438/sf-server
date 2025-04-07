using SFServer.Shared.Models.Inventory;
using SFServer.Shared.Models.Wallet;

namespace SFServer.UI.Models.Economy
{
    public class EconomyDisplayViewModel
    {
        public List<Currency> Currencies { get; set; } = [];
        public List<InventoryItem> InventoryItems { get; set; } = [];
    }
}