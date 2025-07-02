using System.Collections.Generic;
using System.Linq;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using SFServer.Shared.Server.Analytics;

namespace SFServer.API.Services;

public class ClickHouseAnalyticsService : IAnalyticsService, IDisposable
{
    private readonly ClickHouseConnection _connection;

    public ClickHouseAnalyticsService(string connectionString)
    {
        _connection = new ClickHouseConnection(connectionString);
        _connection.Open();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS analytics_events
( 
    Id String,
    Params Map(String, String),
    Timestamp DateTime
) ENGINE = MergeTree() ORDER BY (Timestamp)";
        cmd.ExecuteNonQuery();
    }

    public Task InsertEventAsync(AnalyticsEventDto evt) => InsertEventsAsync(new[] { evt });

    public async Task InsertEventsAsync(IEnumerable<AnalyticsEventDto> events)
    {
        var list = events.ToList();
        if (list.Count == 0) return;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT INTO analytics_events VALUES (@ids, @params, @ts)";
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "ids", Value = list.Select(e => e.Id).ToArray() });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "params", Value = list.Select(e => e.Params).ToArray() });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "ts", Value = list.Select(e => e.Timestamp).ToArray() });
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose() => _connection.Dispose();
}
