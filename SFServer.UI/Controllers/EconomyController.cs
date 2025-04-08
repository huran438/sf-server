using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Models.Inventory;
using SFServer.Shared.Models.Wallet;
using SFServer.UI.Models;
using SFServer.UI.Models.Economy;

namespace SFServer.UI.Controllers
{
    public class EconomyController : Controller
    {
        private readonly IConfiguration _configuration;

        public EconomyController(IConfiguration configuration)
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

        public async Task<IActionResult> Index(string activeTab)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            var currencies = await httpClient.GetFromJsonAsync<List<Currency>>("Currency");
            var inventoryItems = await httpClient.GetFromJsonAsync<List<InventoryItem>>("InventoryItems");

            var model = new EconomyDisplayViewModel
            {
                Currencies = currencies ?? new List<Currency>(),
                InventoryItems = inventoryItems ?? new List<InventoryItem>()
            };

            // Optionally store activeTab in ViewBag (for server-side logging)
            ViewBag.ActiveTab = activeTab ?? "wallet";

            return View(model);
        }


        #region Currency

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCurrency(CurrencyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the Index view with an appropriate display model if needed.
                return View("Index");
            }

            var currency = new Currency
            {
                Title = model.NewCurrencyTitle,
                Icon = model.NewCurrencyIcon,
                RichText = model.NewCurrencyRichText,
                InitialAmount = model.NewCurrencyInitialAmount ?? 0,
                Capacity = model.NewCurrencyCapacity ?? 0,
                RefillSeconds = model.NewCurrencyRefillSeconds ?? 0,
                Color = model.NewCurrencyColorHex
            };

            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.PostAsJsonAsync("currency/create", currency);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Failed to add currency: {response.StatusCode}";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Currency added successfully!";
            return RedirectToAction("Index");
        }



        // GET: /Economy/EditCurrency/{id}
        public async Task<IActionResult> EditCurrency(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            var currency = await httpClient.GetFromJsonAsync<Currency>($"Currency/{id}");
            if (currency == null)
                return NotFound();

            var model = new EditCurrencyViewModel
            {
                Id = currency.Id,
                Title = currency.Title,
                Icon = currency.Icon,
                RichText = currency.RichText,
                InitialAmount = currency.InitialAmount,
                Capacity = currency.Capacity,
                RefillSeconds = currency.RefillSeconds,
                ColorHex = currency.Color
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCurrency(EditCurrencyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Build the updated currency object, using ColorHex directly as a string.
            var updatedCurrency = new Currency
            {
                Id = model.Id,
                Title = model.Title,
                Icon = model.Icon ?? string.Empty,
                RichText = model.RichText ?? string.Empty,
                InitialAmount = model.InitialAmount,
                Capacity = model.Capacity,
                RefillSeconds = model.RefillSeconds,
                Color = model.ColorHex ?? string.Empty // directly assign the string
            };

            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.PutAsJsonAsync($"Currency/{model.Id}", updatedCurrency);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Economy");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed to update currency: {errorContent}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCurrency(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.DeleteAsync($"Currency/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete currency.";
            }

            return RedirectToAction("Index");
        }

        #endregion


        #region Inventory

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInventoryItem(InventoryItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", new { activeTab = "inventory" });
            }

            var inventoryItem = new InventoryItem
            {
                Id = Guid.NewGuid(),
                Title = model.Name,
                Description = model.Description,
                Quantity = model.Quantity,
                Type = model.Type,
                ImageUrl = model.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.PostAsJsonAsync("InventoryItems", inventoryItem);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Failed to add inventory item: {response.StatusCode}";
                return RedirectToAction("Index", new { activeTab = "inventory" });
            }

            TempData["Success"] = "Inventory item added successfully!";
            // Redirect to Index with activeTab=inventory in the query string.
            return RedirectToAction("Index", new { activeTab = "inventory" });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInventoryItem(EditInventoryItemViewModel item)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data submitted.";
                return RedirectToAction("Index", new { activeTab = "inventory" });
            }

            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.PutAsJsonAsync($"InventoryItems/{item.Id}", new InventoryItem
            {
                Id = item.Id,
                Title = item.Name,
                Description = item.Description,
                Quantity = item.Quantity,
                Type = item.Type,
                ImageUrl = item.ImageUrl,
                CreatedAt = item.CreatedAt
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to update inventory item: {errorContent}";
            }
            else
            {
                TempData["Success"] = "Inventory item updated successfully!";
            }

            return RedirectToAction("Index", new { activeTab = "inventory" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInventoryItem(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.DeleteAsync($"InventoryItems/{id}");

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Failed to delete inventory item.";

            return RedirectToAction("Index", new { activeTab = "inventory" });
        }

        #endregion
    }
}