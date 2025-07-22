using Microsoft.AspNetCore.Http;
using SFServer.API.Data;
using SFServer.Shared.Server.Audit;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFServer.API.Utils
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DatabseContext db)
        {
            await _next(context);

            string userIdString = null;
            if (context.Request.Headers.TryGetValue(Headers.UID, out var headerUserId))
            {
                userIdString = headerUserId.ToString();
            }
            if (string.IsNullOrEmpty(userIdString))
            {
                userIdString = context.User?.FindFirstValue("UserId");
            }
            
            Guid projectId = Guid.Empty;
            var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments != null && segments.Length > 0)
            {
                Guid.TryParse(segments[0], out projectId);
            }

            var entry = new AuditLogEntry
            {
                Id = Guid.CreateVersion7(),
                ProjectId = projectId,
                UserId = Guid.TryParse(userIdString, out var g) ? g : null,
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow
            };

            db.AuditLogs.Add(entry);
            await db.SaveChangesAsync();
        }
    }
}
