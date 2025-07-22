using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Purchases;
using SFServer.Shared.Server.Wallet;
using SFServer.Shared.Server.Inventory;
using SFServer.UI;

namespace SFServer.UI.Pages.Purchases
{
    public class EditProductModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        [BindProperty(SupportsGet = true)]
        public Guid projectId { get; set; }
        [BindProperty(SupportsGet = true)]
        public Guid id { get; set; }

        public EditProductModel(IConfiguration config, ProjectContext project)
        {
            _config = config;
            _project = project;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public List<Currency> Currencies { get; set; } = new();
        public List<InventoryItem> InventoryItems { get; set; } = new();

        [TempData]
        public string? ActiveTab { get; set; }

        private HttpClient GetClient() => User.CreateApiClient(_config, _project.CurrentProjectId);

        public async Task OnGetAsync()
        {
            using var http = GetClient();
            Product = await http.GetFromMessagePackAsync<Product>($"Purchases/products/{id}");
            Currencies = await http.GetFromMessagePackAsync<List<Currency>>("Currency");
            InventoryItems = await http.GetFromMessagePackAsync<List<InventoryItem>>("Inventory");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var http = GetClient();
            var response = await http.PutAsMessagePackAsync($"Purchases/products/{id}", Product);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update product");
                return Page();
            }
            ActiveTab = "products";
            return Redirect($"/{projectId}/Economy#products");
        }
    }
}
