using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Wallet;
using SFServer.UI.Models;
using SFServer.UI;

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
            return User.CreateApiClient(_configuration);
        }

        // GET: /Economy/Index
        public async Task<IActionResult> Index()
        {
            using var httpClient = GetAuthenticatedHttpClient();
            // Retrieve currencies using MessagePack
            var currencies = await httpClient.GetFromMessagePackAsync<List<Currency>>("Currency");
            var model = new EconomyViewModel
            {
                Currencies = currencies ?? new List<Currency>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCurrency(EconomyViewModel model)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Build the Currency object.
            var currency = new Currency
            {
                Title = model.NewCurrencyTitle,
                Icon = string.IsNullOrWhiteSpace(model.NewCurrencyIcon) ? string.Empty : model.NewCurrencyIcon,
                RichText = string.IsNullOrWhiteSpace(model.NewCurrencyRichText) ? string.Empty : model.NewCurrencyRichText,
                InitialAmount = model.NewCurrencyInitialAmount ?? 0,
                Capacity = model.NewCurrencyCapacity ?? 0,
                RefillSeconds = model.NewCurrencyRefillSeconds ?? 0,
                Color = string.IsNullOrWhiteSpace(model.NewCurrencyIcon) ? "#FFFFFF" : model.NewCurrencyIcon,
            };

            using var httpClient = GetAuthenticatedHttpClient();
            // Post the currency object using MessagePack.
            var response = await httpClient.PostMessagePackAsync("Currency/create", currency);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Failed to add currency: {response.StatusCode}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Economy/EditCurrency/{id}
        public async Task<IActionResult> EditCurrency(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            // Retrieve the currency using MessagePack.
            Currency currency;
            try
            {
                currency = await httpClient.GetFromMessagePackAsync<Currency>($"Currency/{id}");
            }
            catch (ApiRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (ApiRequestException ex)
            {
                TempData["Error"] = $"Failed to load currency: {ex.Message}";
                return RedirectToAction("Index");
            }

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

            // Build the updated currency object.
            var updatedCurrency = new Currency
            {
                Id = model.Id,
                Title = model.Title,
                Icon = model.Icon ?? string.Empty,
                RichText = model.RichText ?? string.Empty,
                InitialAmount = model.InitialAmount,
                Capacity = model.Capacity,
                RefillSeconds = model.RefillSeconds,
                Color = model.ColorHex ?? string.Empty
            };

            using var httpClient = GetAuthenticatedHttpClient();
            // Use MessagePack for the PUT update.
            var response = await httpClient.PutAsMessagePackAsync($"Currency/{model.Id}", updatedCurrency);
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
    }
}