using MemoryPack;

namespace SFServer.Shared.Server.Statistics
{
    [MemoryPackable]
    public partial class StatisticsDto
    {
        public int TotalUserCount { get; set; }
        public int MonthlyActiveUsers { get; set; }
        public int DailyActiveUsers { get; set; }
        public double RetentionRate { get; set; }
    }
}
