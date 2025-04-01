using System.ComponentModel.DataAnnotations;
using System.Drawing;
using SecretGameBackend.Shared.Models;

namespace SFServer.UI.Models
{
    public class EditCurrencyViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        
        public string? Icon { get; set; } = string.Empty;
        public string? RichText { get; set; } = string.Empty;
        
        [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater.")]
        public int InitialAmount { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater.")]
        public int Capacity { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater.")]
        public int RefillSeconds { get; set; }
        public string? ColorHex { get; set; }
    }
}