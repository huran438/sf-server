using System.ComponentModel.DataAnnotations;

namespace SFServer.UI.Models.UserProfiles
{
    public class LoginViewModel
    {
        [Required]
        public string Credential { get; set; }
        
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool AdminPanel => true;

        public string ReturnUrl { get; set; }
    }
}