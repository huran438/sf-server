using SFServer.Shared.Server.Analytics;

namespace SFServer.API.Services;

public interface IAnalyticsService
{
    Task InsertEventAsync(AnalyticsEventDto evt);
    Task InsertEventsAsync(IEnumerable<AnalyticsEventDto> events);
}
