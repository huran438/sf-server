using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFServer.UI;
using System.Collections.Generic;

namespace SFServer.UI.Pages.Inventory
{
    public class CreateInventoryItemModel : PageModel
    {
        private readonly IConfiguration _config;

        public CreateInventoryItemModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public InventoryItem Item { get; set; } = new();

        [BindProperty]
        public string Tags { get; set; }

        [BindProperty]
        public string DropJson { get; set; }

        private HttpClient GetClient()
        {
            return User.CreateApiClient(_config);
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

            if (!string.IsNullOrWhiteSpace(DropJson))
            {
                try
                {
                    Item.Drop = System.Text.Json.JsonSerializer.Deserialize<List<InventoryDropEntry>>(DropJson) ?? new();
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Invalid drop JSON");
                    return Page();
                }
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
