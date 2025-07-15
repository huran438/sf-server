using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using SFServer.Shared.Server.Wallet;
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

        public IEnumerable<InventoryItem> GetItemOptions(Guid? current)
        {
            var used = DropEntries
                .Where(d => d.Type == "Item" && d.ItemId.HasValue && d.ItemId != current)
                .Select(d => d.ItemId!.Value)
                .ToHashSet();
            return Items.Where(i => !used.Contains(i.Id));
        }

        public IEnumerable<Currency> GetCurrencyOptions(Guid? current)
        {
            var used = DropEntries
                .Where(d => d.Type == "Currency" && d.CurrencyId.HasValue && d.CurrencyId != current)
                .Select(d => d.CurrencyId!.Value)
                .ToHashSet();
            return Currencies.Where(c => !used.Contains(c.Id));
        }

        public bool CanAddDrop => GetItemOptions(null).Any() || GetCurrencyOptions(null).Any();

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

        public async Task OnGetAsync(Guid id)
        {
            using var http = GetClient();
            Item = await http.GetFromMessagePackAsync<InventoryItem>($"Inventory/{id}");
            Tags = Item.Tags != null && Item.Tags.Count > 0 ? string.Join(", ", Item.Tags) : string.Empty;
            DropEntries = Item.Drop.Select(d => new DropEntryVm
            {
                Type = d.ItemId.HasValue ? "Item" : "Currency",
                ItemId = d.ItemId,
                CurrencyId = d.CurrencyId,
                Amount = d.Amount
            }).ToList();
            await LoadListsAsync();
        }

        public async Task<IActionResult> OnPostAddDropAsync()
        {
            await LoadListsAsync();
            DropEntries ??= new();

            var availableItems = GetItemOptions(null).ToList();
            var availableCurrencies = GetCurrencyOptions(null).ToList();

            if (availableItems.Count == 0 && availableCurrencies.Count == 0)
            {
                ModelState.Clear();
                return Page();
            }

            var entry = new DropEntryVm();
            if (availableItems.Count > 0)
            {
                entry.Type = "Item";
                entry.ItemId = availableItems.First().Id;
            }
            else
            {
                entry.Type = "Currency";
                entry.CurrencyId = availableCurrencies.First().Id;
            }

            DropEntries.Add(entry);
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
            else
            {
                Item.Tags = new List<string>();
            }

            if (DropEntries != null)
            {
                Item.Drop = DropEntries
                    .Select(d => d.Type == "Item"
                        ? new InventoryDropEntry { ItemId = d.ItemId, Amount = d.Amount }
                        : new InventoryDropEntry { CurrencyId = d.CurrencyId, Amount = d.Amount })
                    .GroupBy(d => new { d.ItemId, d.CurrencyId })
                    .Select(g => g.First())
                    .ToList();
            }

            var resp = await http.PutAsMessagePackAsync($"Inventory/{Item.Id}", Item);
            if (!resp.IsSuccessStatusCode)
            {
                await LoadListsAsync();
                ModelState.AddModelError(string.Empty, "Failed to save item");
                return Page();
            }
            return RedirectToPage("/Inventory/Index");
        }
    }
}
