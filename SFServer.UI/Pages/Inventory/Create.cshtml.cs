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
    public class CreateInventoryItemModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        [BindProperty(SupportsGet = true)]
        public Guid projectId { get; set; }

        public CreateInventoryItemModel(IConfiguration config, ProjectContext project)
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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var http = GetClient();

            var response = await http.PostMessagePackAsync("Inventory", Item);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create item");
                return Page();
            }
            return RedirectToPage("/Inventory/Index", new { projectId });
        }
    }
}
