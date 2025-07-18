using System;
using System.Collections.Generic;
using SFServer.Shared.Server.Admin;
using SFServer.Shared.Server.Project;
using SFServer.Shared.Server.Settings;

namespace SFServer.UI.Models
{
    public class GlobalSettingsViewModel
    {
        public GlobalSettings Settings { get; set; } = new();
        public List<Administrator> Administrators { get; set; } = new();
        public List<ProjectInfo> Projects { get; set; } = new();
    }
}
