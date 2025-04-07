using System.ComponentModel.DataAnnotations;

namespace SFServer.UI.Models.Economy
{
    public class InventoryItemFormModel
    {
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Type { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}