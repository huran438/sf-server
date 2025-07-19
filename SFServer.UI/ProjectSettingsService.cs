using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Settings;

namespace SFServer.UI
{
    public class ProjectSettingsService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;
        private readonly ProjectContext _project;
        private ProjectSettings _cached;
        private Guid _cachedProjectId;

        public ProjectSettingsService(IHttpClientFactory factory, IConfiguration configuration, ProjectContext project)
        {
            _factory = factory;
            _configuration = configuration;
            _project = project;
        }

        public async Task<ProjectSettings> GetSettingsAsync()
        {
            if (_cached != null && _cachedProjectId == _project.CurrentProjectId)
                return _cached;

            var client = _factory.CreateClient("api");
            if (_project.CurrentProjectId != Guid.Empty)
                client.BaseAddress = new Uri(client.BaseAddress.ToString().TrimEnd('/') + "/" + _project.CurrentProjectId + "/");

            _cached = await client.GetFromMessagePackAsync<ProjectSettings>("ProjectSettings") ?? new ProjectSettings();
            _cachedProjectId = _project.CurrentProjectId;
            return _cached;
        }

        public void UpdateCache(ProjectSettings settings)
        {
            _cached = settings;
            _cachedProjectId = _project.CurrentProjectId;
        }
    }
}
