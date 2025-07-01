using SFServer.Shared.Server.Settings;

namespace SFServer.UI;

public class ServerSettingsService
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _configuration;
    private ServerSettings? _cached;

    public ServerSettingsService(IHttpClientFactory factory, IConfiguration configuration)
    {
        _factory = factory;
        _configuration = configuration;
    }

    public async Task<ServerSettings?> GetSettingsAsync()
    {
        if (_cached != null)
            return _cached;
        var client = _factory.CreateClient("api");
        _cached = await client.GetFromMessagePackAsync<ServerSettings>("ServerSettings");
        return _cached;
    }

    public void UpdateCache(ServerSettings settings)
    {
        _cached = settings;
    }
}
