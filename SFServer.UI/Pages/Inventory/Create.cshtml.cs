using System;
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
    public class CreateInventoryItemModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        public CreateInventoryItemModel(IConfiguration config, ProjectContext project)
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

        public void OnGet()
        {
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

            var response = await http.PostMessagePackAsync("Inventory", Item);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create item");
                return Page();
            }
            return RedirectToPage("/Inventory/Index");
        }
    }
}
