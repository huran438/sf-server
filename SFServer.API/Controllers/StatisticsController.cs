using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Statistics;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("{projectId:guid}/[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class StatisticsController : ControllerBase
    {
        private readonly DatabseContext _db;

        public StatisticsController(DatabseContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics(Guid projectId)
        {

            var totalUsers = await _db.UserProfiles.CountAsync(u => u.ProjectId == projectId);
            var now = DateTime.UtcNow;
            var mau = await _db.UserProfiles.CountAsync(u => u.ProjectId == projectId && u.LastLoginAt >= now.AddDays(-30));
            var dau = await _db.UserProfiles.CountAsync(u => u.ProjectId == projectId && u.LastLoginAt >= now.AddDays(-1));
            double retention = mau == 0 ? 0 : (double)dau / mau;

            var dto = new StatisticsDto
            {
                TotalUserCount = totalUsers,
                MonthlyActiveUsers = mau,
                DailyActiveUsers = dau,
                RetentionRate = retention
            };

            return Ok(dto);
        }
    }
}
