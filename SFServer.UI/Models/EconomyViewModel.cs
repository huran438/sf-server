using System.ComponentModel.DataAnnotations;
using SFServer.Shared.Models.Wallet;

namespace SFServer.UI.Models
{
    public class EconomyViewModel
    {
        public List<Currency> Currencies { get; set; } = new List<Currency>();

        // Only Title is required.
        [Required(ErrorMessage = "Title is required.")]
        public string NewCurrencyTitle { get; set; }

        // Optional fields: if not provided, we'll default to empty string or zero.
        public string? NewCurrencyIcon { get; set; } = string.Empty;
        public string? NewCurrencyRichText { get; set; } = string.Empty;

        // Use nullable ints so that if left blank, we can assign a default value.
        public int? NewCurrencyInitialAmount { get; set; }
        public int? NewCurrencyCapacity { get; set; }
        public int? NewCurrencyRefillSeconds { get; set; }
        public string? NewCurrencyColorHex { get; set; } = "#FFFFFF";
    }
}