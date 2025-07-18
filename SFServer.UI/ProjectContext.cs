using System;

namespace SFServer.UI;

public class ProjectContext
{
    public Guid CurrentProjectId { get; set; } = Guid.Empty;
    public string CurrentProjectName { get; set; } = string.Empty;
}
