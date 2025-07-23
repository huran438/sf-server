using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFServer.Shared.Server.Dashboard;

namespace SFServer.UI
{
    public class DashboardMetricsService
    {
        public Task<DashboardMetricsDTO> GetMetricsAsync()
        {
            var rnd = new Random();
            var dto = new DashboardMetricsDTO
            {
                DAU = rnd.Next(5000, 15000),
                D1Retention = rnd.NextDouble() * 0.5 + 0.3,
                RevenueToday = (decimal)(rnd.NextDouble() * 1000),
                ARPU = (decimal)(rnd.NextDouble() * 2),
                PayingPercent = rnd.NextDouble() * 0.1 + 0.02,
                ROAS = rnd.NextDouble() * 2,
                RetentionSeries = new List<double> {0.4,0.35,0.3,0.28,0.27,0.25,0.23},
                RevenueDates = new List<DateTime>(),
                RevenueIap = new List<decimal>(),
                RevenueAds = new List<decimal>(),
                MonetizationFunnel = new List<int>{10000,5000,1200,300},
                LtvCohorts = new List<List<decimal>>
                {
                    new() {1m,1.5m,2m},
                    new() {1.1m,1.6m,2.1m},
                    new() {1.2m,1.7m,2.2m}
                },
                GeoSplit = new Dictionary<string, int>{{"US",40},{"EU",30},{"JP",30}},
                OnboardingFunnel = new List<int>{10000,8000,6000,1200},
                CrashTypes = new Dictionary<string, int>{{"NullRef",5},{"Timeout",3},{"Other",2}},
                CrashTraces = new List<string>{"StackTrace 1","StackTrace 2","StackTrace 3"},
                LiveOpsEvent = new LiveOpsEventDTO
                {
                    Title = "Summer Fest",
                    Start = DateTime.UtcNow.AddDays(-2),
                    End = DateTime.UtcNow.AddDays(5),
                    EngagementPercent = 0.45,
                    Revenue = 2500m
                }
            };
            for (int i=0;i<30;i++)
            {
                dto.RevenueDates.Add(DateTime.UtcNow.Date.AddDays(-29+i));
                dto.RevenueIap.Add((decimal)(rnd.NextDouble()*200));
                dto.RevenueAds.Add((decimal)(rnd.NextDouble()*100));
            }
            return Task.FromResult(dto);
        }
    }
}
