using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Audit;
using SFServer.Shared.Server.UserProfile;
using SFServer.UI.Models.AuditLogs;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IConfiguration _config;

        public AuditLogController(IConfiguration config)
        {
            _config = config;
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_config["API_BASE_URL"]) };
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<IActionResult> Index()
        {
            using var client = GetClient();

            var logs = await client.GetFromMessagePackAsync<List<AuditLogEntry>>("AuditLog?count=100");
            var profiles = await client.GetFromMessagePackAsync<List<UserProfile>>("UserProfiles");
            var profileLookup = profiles.ToDictionary(p => p.Id);

            var groups = new Dictionary<UserRole, List<AuditLogEntry>>();
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                groups[role] = new List<AuditLogEntry>();
            }

            foreach (var log in logs)
            {
                var role = UserRole.Guest;
                if (log.UserId.HasValue && profileLookup.TryGetValue(log.UserId.Value, out var profile))
                {
                    role = profile.Role;
                }
                groups[role].Add(log);
            }

            var model = new AuditLogIndexViewModel
            {
                Groups = groups
                    .Where(kv => kv.Value.Count > 0)
                    .Select(kv => new AuditLogRoleGroup { Role = kv.Key, Logs = kv.Value.OrderByDescending(l => l.Timestamp).ToList() })
                    .ToList()
            };

            return View(model);
        }
    }
}
