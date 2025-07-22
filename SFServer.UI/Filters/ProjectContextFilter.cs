using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Project;

namespace SFServer.UI.Filters
{
    public class ProjectContextFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _context;

        public ProjectContextFilter(IConfiguration config, ProjectContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Read project id from route if provided
            if (context.RouteData.Values.TryGetValue("projectId", out var value) &&
                Guid.TryParse(value?.ToString(), out var id))
            {
                _context.CurrentProjectId = id;
            }

            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                using var client = context.HttpContext.User.CreateApiClient(_config);
                var projects = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects") ?? new();
                if (projects.Count > 0)
                {
                    var match = projects.FirstOrDefault(p => p.Id == _context.CurrentProjectId);
                    if (match == null)
                    {
                        match = projects.First();
                    }
                    _context.CurrentProjectId = match.Id;
                    _context.CurrentProjectName = match.Name;
                }
                else
                {
                    _context.CurrentProjectId = Guid.Empty;
                    _context.CurrentProjectName = string.Empty;
                }
            }

            await next();
        }
    }
}
