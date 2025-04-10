using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Wallet;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class CurrencyController : ControllerBase
    {
        private readonly UserProfilesDbContext _context;
        public CurrencyController(UserProfilesDbContext context)
        {
            _context = context;
        }
        
        // GET /Currency
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currencies = await _context.Currencies.ToListAsync();
            return Ok(currencies);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCurrencyById(Guid id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null)
            {
                return NotFound();
            }
            return Ok(currency);
        }
        
        // POST /Currency/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Currency currency)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = currency.Id }, currency);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrency(Guid id, [FromBody] Currency updatedCurrency)
        {
            if (id != updatedCurrency.Id)
                return BadRequest("ID mismatch.");

            var existingCurrency = await _context.Currencies.FindAsync(id);
            if (existingCurrency == null)
                return NotFound("Currency not found.");

            // Update all properties.
            existingCurrency.Title = updatedCurrency.Title;
            existingCurrency.Icon = updatedCurrency.Icon;
            existingCurrency.RichText = updatedCurrency.RichText;
            existingCurrency.InitialAmount = updatedCurrency.InitialAmount;
            existingCurrency.Capacity = updatedCurrency.Capacity;
            existingCurrency.RefillSeconds = updatedCurrency.RefillSeconds;
            existingCurrency.Color = updatedCurrency.Color;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // DELETE /Currency/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurrency(Guid id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null)
            {
                return NotFound("Currency not found.");
            }
            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}