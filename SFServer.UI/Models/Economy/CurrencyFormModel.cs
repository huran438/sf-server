using System.ComponentModel.DataAnnotations;

namespace SFServer.UI.Models.Economy
{
    public class CurrencyFormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        public string NewCurrencyTitle { get; set; } = string.Empty;

        public string NewCurrencyIcon { get; set; } = string.Empty;

        public string NewCurrencyRichText { get; set; } = string.Empty;

        public int? NewCurrencyInitialAmount { get; set; }

        public int? NewCurrencyCapacity { get; set; }

        public int? NewCurrencyRefillSeconds { get; set; }

        public string NewCurrencyColorHex { get; set; } = "#FFFFFF";
    }
}