using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFServer.UI;

namespace SFServer.UI.Pages.Inventory
{
    public class EditInventoryItemModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        [BindProperty(SupportsGet = true)]
        public Guid projectId { get; set; }

        public EditInventoryItemModel(IConfiguration config, ProjectContext project)
        {
            _config = config;
            _project = project;
        }

        [BindProperty]
        public InventoryItem Item { get; set; } = new();


        private HttpClient GetClient()
        {
            return User.CreateApiClient(_config, _project.CurrentProjectId);
        }

        public async Task OnGetAsync(Guid id)
        {
            using var http = GetClient();
            Item = await http.GetFromMessagePackAsync<InventoryItem>($"Inventory/{id}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var http = GetClient();

            await http.PutAsMessagePackAsync($"Inventory/{Item.Id}", Item);
            return RedirectToPage("/Inventory/Index", new { projectId });
        }
    }
}
