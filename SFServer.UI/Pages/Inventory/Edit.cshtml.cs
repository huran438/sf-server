using System;
using System.Collections.Generic;
using System.Linq;
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

        public EditInventoryItemModel(IConfiguration config, ProjectContext project)
        {
            _config = config;
            _project = project;
        }

        [BindProperty]
        public InventoryItem Item { get; set; } = new();

        [BindProperty]
        public string Tags { get; set; }

        private HttpClient GetClient()
        {
            return User.CreateApiClient(_config, _project.CurrentProjectId);
        }

        public async Task OnGetAsync(Guid id)
        {
            using var http = GetClient();
            Item = await http.GetFromMessagePackAsync<InventoryItem>($"Inventory/{id}");
            Tags = Item.Tags != null && Item.Tags.Count > 0 ? string.Join(", ", Item.Tags) : string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var http = GetClient();

            if (!string.IsNullOrWhiteSpace(Tags))
            {
                Item.Tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            }
            else
            {
                Item.Tags = new List<string>();
            }

            await http.PutAsMessagePackAsync($"Inventory/{Item.Id}", Item);
            return RedirectToPage("/Inventory/Index");
        }
    }
}
