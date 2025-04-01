using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using SecretGameBackend.Shared.Models.Wallet;

namespace SFServer.UI.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IConfiguration _configuration;

        public WalletController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        private HttpClient GetAuthenticatedHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_configuration["API_BASE_URL"]) };
            var jwtToken = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }
            else
            {
                Console.WriteLine("JWT token not found in user claims.");
            }
            return client;
        }
        
        public async Task<IActionResult> Index()
        {
            // Use Guid for user ID.
            Guid userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            using var client = GetAuthenticatedHttpClient();
            var walletItems = await client.GetFromJsonAsync<List<WalletItem>>($"Wallet/{userId}");
            return View(walletItems);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWalletItem(Guid walletItemId, decimal amount)
        {
            using var client = GetAuthenticatedHttpClient();
            // Build a minimal object with the ID and new amount.
            var updatePayload = new { Id = walletItemId, Amount = amount };
            var response = await client.PutAsJsonAsync($"Wallet/{walletItemId}", updatePayload);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to update wallet item.";
            }
            return RedirectToAction("Index");
        }
    }
}
