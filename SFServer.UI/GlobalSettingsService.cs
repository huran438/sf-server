using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Settings;

namespace SFServer.UI
{
    public class GlobalSettingsService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;
        private GlobalSettings _cached;

        public GlobalSettingsService(IHttpClientFactory factory, IConfiguration configuration)
        {
            _factory = factory;
            _configuration = configuration;
        }

        public async Task<GlobalSettings> GetSettingsAsync()
        {
            if (_cached != null)
                return _cached;

            var client = _factory.CreateClient("api");
            _cached = await client.GetFromMessagePackAsync<GlobalSettings>("GlobalSettings") ?? new GlobalSettings();
            return _cached;
        }

        public void UpdateCache(GlobalSettings settings)
        {
            _cached = settings;
        }
    }
}
