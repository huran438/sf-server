using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Statistics;
using SFServer.UI;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class StatisticsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectContext _project;

        public StatisticsController(IConfiguration configuration, ProjectContext project)
        {
            _configuration = configuration;
            _project = project;
        }

        private HttpClient GetAuthenticatedHttpClient()
        {
            return User.CreateApiClient(_configuration, _project.CurrentProjectId);
        }

        public async Task<IActionResult> Index()
        {
            using var client = GetAuthenticatedHttpClient();
            var stats = await client.GetFromMessagePackAsync<StatisticsDto>("Statistics");
            return View(stats);
        }
    }
}
