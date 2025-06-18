using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using System.Net.Http.Headers;
using SFServer.UI;

namespace SFServer.UI.Pages.Inventory;

public class InventoryPageModel : PageModel
{
    private readonly IConfiguration _config;

    public InventoryPageModel(IConfiguration config)
    {
        _config = config;
    }

    public List<InventoryItem> Items { get; set; } = new();
    public List<SFServer.Shared.Server.Wallet.Currency> Currencies { get; set; } = new();

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
        Currencies = await http.GetFromMessagePackAsync<List<SFServer.Shared.Server.Wallet.Currency>>("Currency");
    }
}
