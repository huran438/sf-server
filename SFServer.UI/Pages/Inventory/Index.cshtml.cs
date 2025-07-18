using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Inventory;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFServer.UI;

namespace SFServer.UI.Pages.Inventory
{
    public class InventoryPageModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        [BindProperty(SupportsGet = true)]
        public Guid projectId { get; set; }

        public InventoryPageModel(IConfiguration config, ProjectContext project)
        {
            _config = config;
            _project = project;
        }

        public List<InventoryItem> Items { get; set; } = new();

        private HttpClient GetClient()
        {
            return User.CreateApiClient(_config, _project.CurrentProjectId);
        }

        public async Task OnGetAsync()
        {
            using var http = GetClient();
            Items = await http.GetFromMessagePackAsync<List<InventoryItem>>("Inventory");
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            using var http = GetClient();
            await http.DeleteAsync($"Inventory/{id}");
            return RedirectToPage(new { projectId = projectId });
        }
    }
}
