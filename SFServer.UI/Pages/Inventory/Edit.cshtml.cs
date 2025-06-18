using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using System.Net.Http.Headers;
using SFServer.UI;
using SFServer.UI.Models.Inventory;

namespace SFServer.UI.Pages.Inventory;

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
    public string? Tags { get; set; }

    [BindProperty]
    public List<PriceEntry> PriceEntries { get; set; } = new();

    public List<SFServer.Shared.Server.Wallet.Currency> AllCurrencies { get; set; } = new();

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
        AllCurrencies = await http.GetFromMessagePackAsync<List<SFServer.Shared.Server.Wallet.Currency>>("Currency");
        PriceEntries = Item.Prices.Select(p => new PriceEntry { CurrencyId = p.Key, Amount = p.Value }).ToList();
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

        Item.Prices = PriceEntries
            .Where(p => p.CurrencyId != Guid.Empty)
            .ToDictionary(p => p.CurrencyId, p => p.Amount);

        await http.PutAsMessagePackAsync($"Inventory/{Item.Id}", Item);
        return RedirectToPage("/Inventory/Index");
    }
}
