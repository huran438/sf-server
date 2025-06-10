using SFServer.Shared.Client.ServerSettings;

namespace SFServer.UI.Models
{
    public class ServerSettingsViewModel
    {
        public S3SettingsDto S3 { get; set; } = new S3SettingsDto();
    }
}
