using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using System.Net.Http.Headers;
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

        private HttpClient GetClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_config["API_BASE_URL"]) };
            var token = User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
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
