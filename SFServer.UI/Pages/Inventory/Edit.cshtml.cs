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

        public EditInventoryItemModel(IConfiguration config)
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

        public async Task OnGetAsync(Guid id)
        {
            using var http = GetClient();
            Item = await http.GetFromMessagePackAsync<InventoryItem>($"Inventory/{id}");
            Tags = Item.Tags != null && Item.Tags.Count > 0 ? string.Join(", ", Item.Tags) : string.Empty;
            DropJson = Item.Drop != null && Item.Drop.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(Item.Drop) : string.Empty;
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

            await http.PutAsMessagePackAsync($"Inventory/{Item.Id}", Item);
            return RedirectToPage("/Inventory/Index");
        }
    }
}
