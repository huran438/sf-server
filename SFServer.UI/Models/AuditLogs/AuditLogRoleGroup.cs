using System.Collections.Generic;
using SFServer.Shared.Server.Audit;
using SFServer.Shared.Server.UserProfile;

namespace SFServer.UI.Models.AuditLogs
{
    public class AuditLogRoleGroup
    {
        public UserRole Role { get; set; }
        public List<AuditLogEntry> Logs { get; set; } = new();
    }
}
