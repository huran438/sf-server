using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFServer.Shared.Server.Inventory;
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

    private HttpClient GetClient()
    {
        var client = new HttpClient { BaseAddress = new Uri(_config["API_BASE_URL"]) };
        var token = User.FindFirst("JwtToken")?.Value;
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using var http = GetClient();
        var response = await http.PostMessagePackAsync("Inventory", Item);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create item");
            return Page();
        }
        return RedirectToPage("/Inventory/Index");
    }
}
