﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Wallet;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // Requires authentication.
    public class WalletController : ControllerBase
    {
        private readonly DatabseContext _context;
        public WalletController(DatabseContext context)
        {
            _context = context;
        }
        
        // GET /Wallet/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWallet(Guid userId)
        {
            // Retrieve all system currencies.
            var currencies = await _context.Currencies.ToListAsync();

            // Retrieve existing wallet items for the user.
            var walletItems = await _context.WalletItems
                .Where(w => w.UserId == userId)
                .Include(w => w.Currency)
                .ToListAsync();

            // For each currency not present in the user's wallet, create a default entry.
            foreach (var currency in currencies)
            {
                if (!walletItems.Any(w => w.CurrencyId == currency.Id))
                {
                    var newItem = new WalletItem
                    {
                        UserId = userId,
                        CurrencyId = currency.Id,
                        Amount = currency.InitialAmount, // Default initial amount.
                        Currency = currency
                    };
                    _context.WalletItems.Add(newItem);
                    walletItems.Add(newItem);
                }
            }
            await _context.SaveChangesAsync();

            return Ok(walletItems);
        }
        
        // PUT /Wallet/{walletItemId}
        [HttpPut("{walletItemId:guid}")]
        public async Task<IActionResult> UpdateWalletItem(Guid walletItemId, [FromBody] WalletUpdateDto updateDto)
        {
            if (walletItemId != updateDto.Id)
            {
                Console.WriteLine("ID mismatch.");
                return BadRequest("ID mismatch.");
            }
               

            var existingItem = await _context.WalletItems.FindAsync(walletItemId);
            if (existingItem == null)
            {
                Console.WriteLine("Wallet item not found.");
                return NotFound("Wallet item not found.");
            }
               

            existingItem.Amount = updateDto.Amount;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // PUT /Wallet/batch
        [HttpPut("batch")]
        public async Task<IActionResult> UpdateWalletItems([FromBody] List<WalletUpdateDto> updateDtos)
        {
            if (updateDtos == null || updateDtos.Count == 0)
            {
                return BadRequest("No wallet items provided for update.");
            }

            foreach (var updateDto in updateDtos)
            {
                // Retrieve the existing item by its ID (from the update DTO).
                var existingItem = await _context.WalletItems.FindAsync(updateDto.Id);
                if (existingItem == null)
                {
                    Console.WriteLine($"Wallet item not found for ID: {updateDto.Id}");
                    return NotFound($"Wallet item not found for ID: {updateDto.Id}");
                }

                // Update the amount.
                existingItem.Amount = updateDto.Amount;
            }

            // Save changes once all items have been updated.
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
