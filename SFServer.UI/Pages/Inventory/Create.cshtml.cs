using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using SFServer.Shared.Server.Wallet;
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
        public List<DropEntryVm> DropEntries { get; set; } = new();

        public List<InventoryItem> Items { get; set; } = new();
        public List<Currency> Currencies { get; set; } = new();

        public class DropEntryVm
        {
            public string Type { get; set; } = "Item";
            public Guid? ItemId { get; set; }
            public Guid? CurrencyId { get; set; }
            public int Amount { get; set; } = 1;
        }

        private HttpClient GetClient()
        {
            return User.CreateApiClient(_config);
        }

        private async Task LoadListsAsync()
        {
            using var http = GetClient();
            Items = await http.GetFromMessagePackAsync<List<InventoryItem>>("Inventory");
            Currencies = await http.GetFromMessagePackAsync<List<Currency>>("Currency");
        }

        public async Task OnGetAsync()
        {
            await LoadListsAsync();
        }

        public async Task<IActionResult> OnPostAddDropAsync()
        {
            await LoadListsAsync();
            DropEntries ??= new();
            DropEntries.Add(new DropEntryVm { Type = Items.Any() ? "Item" : "Currency", ItemId = Items.FirstOrDefault()?.Id, CurrencyId = Currencies.FirstOrDefault()?.Id, Amount = 1 });
            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveDropAsync(int index)
        {
            await LoadListsAsync();
            if (index >= 0 && index < DropEntries.Count)
                DropEntries.RemoveAt(index);
            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            using var http = GetClient();
            if (!string.IsNullOrWhiteSpace(Tags))
            {
                Item.Tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            }

            if (DropEntries != null)
            {
                Item.Drop = DropEntries.Select(d => new InventoryDropEntry
                {
                    ItemId = d.Type == "Item" ? d.ItemId : null,
                    CurrencyId = d.Type == "Currency" ? d.CurrencyId : null,
                    Amount = d.Amount
                }).ToList();
            }

            var response = await http.PostMessagePackAsync("Inventory", Item);
            if (!response.IsSuccessStatusCode)
            {
                await LoadListsAsync();
                ModelState.AddModelError(string.Empty, "Failed to create item");
                return Page();
            }
            return RedirectToPage("/Inventory/Index");
        }
    }
}
