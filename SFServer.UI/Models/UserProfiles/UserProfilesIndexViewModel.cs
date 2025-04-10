using SFServer.Shared.Server.UserProfile;

namespace SFServer.UI.Models.UserProfiles
{
    public class UserProfilesIndexViewModel
    {
        public List<UserProfile> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
        
        public string SortColumn { get; set; } = "Username";
        public string SortOrder { get; set; } = "desc";
    }
}