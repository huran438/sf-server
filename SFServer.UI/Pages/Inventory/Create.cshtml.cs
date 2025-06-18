using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using SFServer.UI.Models.Inventory;
using System.Net.Http.Headers;

namespace SFServer.UI.Pages.Inventory;

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

    public async Task OnGetAsync()
    {
        using var http = GetClient();
        AllCurrencies = await http.GetFromMessagePackAsync<List<SFServer.Shared.Server.Wallet.Currency>>("Currency");
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

        Item.Prices = PriceEntries
            .Where(p => p.CurrencyId != Guid.Empty)
            .ToDictionary(p => p.CurrencyId, p => p.Amount);

        var response = await http.PostMessagePackAsync("Inventory", Item);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create item");
            return Page();
        }
        return RedirectToPage("/Inventory/Index");
    }
}
