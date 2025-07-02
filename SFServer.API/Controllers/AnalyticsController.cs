using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Analytics;
using SFServer.Shared.Client.Analytics;
using SFServer.API.Services;

namespace SFServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<AnalyticsEventResponse>> PostEvent([FromBody] AnalyticsEventDto evt)
    {
        if (string.IsNullOrEmpty(evt.Id))
            return BadRequest();
        await _service.InsertEventAsync(evt);
        return Accepted(new AnalyticsEventResponse { Accepted = true });
    }

    [HttpPost("batch")]
    public async Task<ActionResult<AnalyticsEventResponse>> PostEvents([FromBody] List<AnalyticsEventDto> events)
    {
        if (events == null || events.Count == 0 || events.Any(e => string.IsNullOrEmpty(e.Id)))
            return BadRequest();
        await _service.InsertEventsAsync(events);
        return Accepted(new AnalyticsEventResponse { Accepted = true });
    }
}
