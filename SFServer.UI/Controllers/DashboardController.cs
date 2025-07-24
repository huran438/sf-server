using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Dashboard;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class DashboardController : Controller
    {
        private readonly DashboardMetricsService _service;
        private readonly ProjectContext _project;

        public DashboardController(DashboardMetricsService service, ProjectContext project)
        {
            _service = service;
            _project = project;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _service.GetMetricsAsync();
            return View(data);
        }

        [HttpPost]
        public IActionResult Refresh()
        {
            return RedirectToAction("Index", new { projectId = _project.CurrentProjectId });
        }
    }
}
