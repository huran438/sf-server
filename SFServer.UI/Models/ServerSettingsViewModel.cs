using System;
using System.ComponentModel.DataAnnotations;

namespace SFServer.UI.Models
{
    public class ServerSettingsViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Copyright")]
        public string ServerCopyright { get; set; } = string.Empty;

        [Display(Name = "Google Client ID")]
        public string GoogleClientId { get; set; } = string.Empty;

        [Display(Name = "Google Client Secret")]
        public string GoogleClientSecret { get; set; } = string.Empty;
        [Display(Name = "ClickHouse Connection")]
        public string ClickHouseConnection { get; set; } = string.Empty;
    }
}
