using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.UI.Models.UserProfiles
{
    public class EditUserProfileViewModel
    {
        public Guid Id { get; set; }
    
        [Required]
        public string Username { get; set; }
    
        [EmailAddress]
        public string Email { get; set; }
    
        [Required]
        public UserRole Role { get; set; }
    
        public string GoogleId { get; set; }
    
        public string AppleId { get; set; }
    
        public string FacebookId { get; set; }

    
        public List<WalletItemViewModel> WalletItems { get; set; } = new() { };
    
        public bool DebugMode { get; set; }

        public string[] DeviceIds { get; set; } = Array.Empty<string>();

        [Required]
        public UserDevice[] UserDevices { get; set; } = Array.Empty<UserDevice>();

        public List<PlayerInventoryItemViewModel> InventoryItems { get; set; } = new() { };
        public List<PlayerPurchaseViewModel> Purchases { get; set; } = new();
    }
}