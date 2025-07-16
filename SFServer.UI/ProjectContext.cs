using System;

namespace SFServer.UI;

public class ProjectContext
{
    public Guid CurrentProjectId { get; set; }
    public string CurrentProjectName { get; set; } = string.Empty;
}
