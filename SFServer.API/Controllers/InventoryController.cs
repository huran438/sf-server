using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Models.Inventory;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly DatabaseContext _context;

    public InventoryController(DatabaseContext context)
    {
        _context = context;
    }
    
    // Get the inventory for a specific user
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetInventory(Guid userId)
    {
        // Check if inventory exists for the user
        var inventory = await _context.Inventories
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.UserId == userId);

        // If inventory doesn't exist, create a new one
        if (inventory == null)
        {
            inventory = new Inventory
            {
                UserId = userId
            };

            // Add the new inventory to the database
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        return Ok(inventory.Items);
    }

    // Add an item to the user's inventory, creating inventory if it doesn't exist
    [HttpPost("{userId}/add")]
    public async Task<IActionResult> AddItem(Guid userId, [FromBody] InventoryItem newItem)
    {
        // Check if inventory exists for the user
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.UserId == userId);

        // If inventory doesn't exist, create a new one
        if (inventory == null)
        {
            inventory = new Inventory
            {
                UserId = userId
            };

            // Add the new inventory to the database
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        // Add the new item to the user's inventory
        newItem.Id = Guid.NewGuid();
        newItem.CreatedAt = DateTime.UtcNow;

        inventory.Items.Add(newItem);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return Ok(newItem);
    }

    // Remove an item from the user's inventory
    [HttpDelete("{userId}/remove/{itemId}")]
    public async Task<IActionResult> RemoveItem(Guid userId, Guid itemId)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.UserId == userId);

        if (inventory == null)
        {
            return NotFound("User's inventory not found.");
        }

        var item = inventory.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            return NotFound("Item not found.");
        }

        inventory.Items.Remove(item);

        await _context.SaveChangesAsync();

        return Ok(item);
    }

    // Update the quantity of an item
    [HttpPut("{userId}/update/{itemId}")]
    public async Task<IActionResult> UpdateItemQuantity(Guid userId, Guid itemId, [FromBody] int newQuantity)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.UserId == userId);

        if (inventory == null)
        {
            return NotFound("User's inventory not found.");
        }

        var item = inventory.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            return NotFound("Item not found.");
        }

        item.Quantity = newQuantity;

        await _context.SaveChangesAsync();

        return Ok(item);
    }
}