using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SFServer.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServerSettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}