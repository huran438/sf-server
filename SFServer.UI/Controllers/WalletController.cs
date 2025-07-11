﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Wallet;

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
            var userIdClaim = User.FindFirst("UserId")?.Value;
            var userId = string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
            using var client = GetAuthenticatedHttpClient();

            // Retrieve wallet items using MessagePack.
            var walletItems = await client.GetFromMessagePackAsync<List<WalletItem>>($"Wallet/{userId}");
            return View(walletItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWalletItem(Guid walletItemId, decimal amount)
        {
            using var client = GetAuthenticatedHttpClient();

            // Build a minimal object with the ID and new amount.
            var updatePayload = new WalletUpdateDto { Id = walletItemId, Amount = amount };

            // Update the wallet item using MessagePack.
            var response = await client.PutAsMessagePackAsync($"Wallet/{walletItemId}", updatePayload);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Failed to update wallet item: {response}";
            }

            return RedirectToAction("Index");
        }
    }
}