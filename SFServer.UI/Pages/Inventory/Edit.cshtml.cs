using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
using System.Net.Http.Headers;
using SFServer.UI;

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
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using var http = GetClient();
        await http.PutAsMessagePackAsync($"Inventory/{Item.Id}", Item);
        return RedirectToPage("/Inventory/Index");
    }
}
