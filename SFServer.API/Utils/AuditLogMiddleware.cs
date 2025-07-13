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

            var userIdString = context.User?.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdString) && context.Request.Headers.TryGetValue("UserId", out var headerUserId))
            {
                userIdString = headerUserId.ToString();
            }
            
            Console.WriteLine("User Id: " + userIdString);

            var entry = new AuditLogEntry
            {
                Id = Guid.CreateVersion7(),
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
