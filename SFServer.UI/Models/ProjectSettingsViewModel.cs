using System;
using System.ComponentModel.DataAnnotations;

namespace SFServer.UI.Models
{
    public class ProjectSettingsViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Google Client ID")]
        public string GoogleClientId { get; set; } = string.Empty;

        [Display(Name = "Google Client Secret")]
        public string GoogleClientSecret { get; set; } = string.Empty;
        [Display(Name = "ClickHouse Connection")]
        public string ClickHouseConnection { get; set; } = string.Empty;

        [Display(Name = "Service Account JSON")]
        public string GoogleServiceAccountJson { get; set; } = string.Empty;

        [Display(Name = "Bundle ID")]
        public string BundleId { get; set; } = string.Empty;
    }
}
