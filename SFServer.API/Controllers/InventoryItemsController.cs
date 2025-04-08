using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Models.Inventory;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class InventoryItemsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public InventoryItemsController(DatabaseContext context)
    {
        _context = context;
    }

    // GET /InventoryItems
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var inventoryItems = await _context.InventoryItems.ToListAsync();
        return Ok(inventoryItems);
    }

    // GET /InventoryItems/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST /InventoryItems
    public async Task<IActionResult> Create([FromBody] InventoryItem item)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        item.Id = Guid.NewGuid();
        item.CreatedAt = DateTime.UtcNow;

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    // PUT /InventoryItems/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] InventoryItem updatedItem)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingItem = await _context.InventoryItems.FindAsync(id);
        if (existingItem == null)
            return NotFound();

        existingItem.Title = updatedItem.Title;
        existingItem.Description = updatedItem.Description;
        existingItem.Quantity = updatedItem.Quantity;
        existingItem.Type = updatedItem.Type;
        existingItem.ImageUrl = updatedItem.ImageUrl;
        // Keep CreatedAt unchanged

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /InventoryItems/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null)
            return NotFound();

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}