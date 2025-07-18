using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Wallet;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("{projectId:guid}/[controller]")]
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
        public async Task<IActionResult> GetWallet(Guid projectId, Guid userId)
        {

            // Retrieve all system currencies for project.
            var currencies = await _context.Currencies
                .Where(c => c.ProjectId == projectId)
                .ToListAsync();

            // Retrieve existing wallet items for the user.
            var walletItems = await _context.WalletItems
                .Where(w => w.UserId == userId && w.ProjectId == projectId)
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
                        Amount = currency.InitialAmount,
                        Currency = currency,
                        ProjectId = projectId
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
        public async Task<IActionResult> UpdateWalletItem(Guid projectId, Guid walletItemId, [FromBody] WalletUpdateDto updateDto)
        {
            if (walletItemId != updateDto.Id)
            {
                Console.WriteLine("ID mismatch.");
                return BadRequest("ID mismatch.");
            }


            var existingItem = await _context.WalletItems
                .FirstOrDefaultAsync(w => w.Id == walletItemId && w.ProjectId == projectId);
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
        public async Task<IActionResult> UpdateWalletItems(Guid projectId, [FromBody] List<WalletUpdateDto> updateDtos)
        {
            if (updateDtos == null || updateDtos.Count == 0)
            {
                return BadRequest("No wallet items provided for update.");
            }


            foreach (var updateDto in updateDtos)
            {
                var existingItem = await _context.WalletItems
                    .FirstOrDefaultAsync(w => w.Id == updateDto.Id && w.ProjectId == projectId);
                if (existingItem == null)
                {
                    Console.WriteLine($"Wallet item not found for ID: {updateDto.Id}");
                    return NotFound($"Wallet item not found for ID: {updateDto.Id}");
                }

                existingItem.Amount = updateDto.Amount;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
