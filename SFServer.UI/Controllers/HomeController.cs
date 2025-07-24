using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using SFServer.UI;

namespace SFServer.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly DashboardMetricsService _metrics;
        private readonly ProjectContext _project;

        public HomeController(DashboardMetricsService metrics, ProjectContext project)
        {
            _metrics = metrics;
            _project = project;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _metrics.GetMetricsAsync();
            return View(data);
        }

        [HttpPost]
        public IActionResult Refresh()
        {
            return RedirectToAction("Index", new { projectId = _project.CurrentProjectId });
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}