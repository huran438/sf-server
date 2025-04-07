using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Models.Wallet;

namespace SFServer.UI.Controllers
{
    [Authorize]
    public abstract class ControllerAuthorize : Controller
    {
        public IConfiguration Configuration => _configuration;

        public HttpClient Client => _client;

        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public ControllerAuthorize(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new HttpClient { BaseAddress = new Uri(_configuration["API_BASE_URL"]) };
            var jwtToken = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }
            else
            {
                Console.WriteLine("JWT token not found in user claims.");
            }
        }
    }

    [Authorize]
    public class WalletController : ControllerAuthorize
    {
        public WalletController(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IActionResult> Index()
        {
            // Use Guid for user ID.
            Guid userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            var walletItems = await Client.GetFromJsonAsync<List<WalletItem>>($"Wallet/{userId}");
            return View(walletItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWalletItem(Guid walletItemId, decimal amount)
        {
            // Build a minimal object with the ID and new amount.
            var updatePayload = new { Id = walletItemId, Amount = amount };
            var response = await Client.PutAsJsonAsync($"Wallet/{walletItemId}", updatePayload);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to update wallet item.";
            }

            return RedirectToAction("Index");
        }
    }
}