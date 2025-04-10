using System.ComponentModel;

namespace SFServer.Shared.Server.UserProfile
{
    public enum UserRole
    {
        [Description("Guest")]
        Guest = 0,
        
        [Description("User")]
        User = 1,

        [Description("Developer")]
        Developer = 2,

        [Description("Administrator")]
        Admin = 3,
    }
}