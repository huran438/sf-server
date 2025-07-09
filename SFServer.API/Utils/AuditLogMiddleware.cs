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

            var entry = new AuditLogEntry
            {
                Id = Guid.CreateVersion7(),
                UserId = context.User?.FindFirstValue("UserId") is string uid && Guid.TryParse(uid, out var g) ? g : null,
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
