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

        public StatisticsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private HttpClient GetAuthenticatedHttpClient()
        {
            return User.CreateApiClient(_configuration);
        }

        public async Task<IActionResult> Index()
        {
            using var client = GetAuthenticatedHttpClient();
            var stats = await client.GetFromMessagePackAsync<StatisticsDto>("Statistics");
            return View(stats);
        }
    }
}
