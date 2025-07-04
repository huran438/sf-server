using SFServer.Shared.Client.Analytics;

namespace SFServer.API.Services
{
    public interface IAnalyticsService
    {
        Task InsertEventAsync(AnalyticsEventDto evt);
        Task InsertEventsAsync(IEnumerable<AnalyticsEventDto> events);
    }
}
