using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Models.Wallet;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // Requires authentication.
    public class WalletController : ControllerBase
    {
        private readonly UserProfilesDbContext _context;
        public WalletController(UserProfilesDbContext context)
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
        [HttpPut("{walletItemId}")]
        public async Task<IActionResult> UpdateWalletItem(Guid walletItemId, [FromBody] WalletUpdateDto updateDto)
        {
            if (walletItemId != updateDto.Id)
                return BadRequest("ID mismatch.");

            var existingItem = await _context.WalletItems.FindAsync(walletItemId);
            if (existingItem == null)
                return NotFound("Wallet item not found.");

            existingItem.Amount = updateDto.Amount;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
