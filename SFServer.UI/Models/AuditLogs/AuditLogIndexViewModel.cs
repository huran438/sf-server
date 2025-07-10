using System.Collections.Generic;

namespace SFServer.UI.Models.AuditLogs
{
    public class AuditLogIndexViewModel
    {
        public List<AuditLogRoleGroup> Groups { get; set; } = new();
    }
}
