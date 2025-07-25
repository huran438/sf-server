using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Server.Inventory;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFServer.UI;

namespace SFServer.UI.Pages.Inventory
{
    public class InventoryPageModel : PageModel
    {
        private readonly IConfiguration _config;

        public InventoryPageModel(IConfiguration config)
        {
            _config = config;
        }

        public List<InventoryItem> Items { get; set; } = new();

        private HttpClient GetClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_config["API_BASE_URL"]) };
            var token = User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
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
            return RedirectToPage();
        }
    }
}
