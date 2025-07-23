using System;
using System.Collections.Generic;
using MemoryPack;

namespace SFServer.Shared.Server.Dashboard
{
    [MemoryPackable]
    public partial class DashboardMetricsDTO
    {
        public int DAU { get; set; }
        public double D1Retention { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal ARPU { get; set; }
        public double PayingPercent { get; set; }
        public double ROAS { get; set; }

        public List<double> RetentionSeries { get; set; } = new();
        public List<DateTime> RevenueDates { get; set; } = new();
        public List<decimal> RevenueIap { get; set; } = new();
        public List<decimal> RevenueAds { get; set; } = new();
        public List<int> MonetizationFunnel { get; set; } = new();
        public List<List<decimal>> LtvCohorts { get; set; } = new();
        public Dictionary<string, int> GeoSplit { get; set; } = new();
        public List<int> OnboardingFunnel { get; set; } = new();
        public Dictionary<string, int> CrashTypes { get; set; } = new();
        public List<string> CrashTraces { get; set; } = new();
        public LiveOpsEventDTO LiveOpsEvent { get; set; } = new();
    }

    [MemoryPackable]
    public partial class LiveOpsEventDTO
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double EngagementPercent { get; set; }
        public decimal Revenue { get; set; }
    }
}
