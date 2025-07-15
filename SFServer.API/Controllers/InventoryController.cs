using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.API.Services;
using SFServer.Shared.Server.Inventory;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _service;

        public InventoryController(InventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _service.GetItemsAsync();
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetItem(Guid id)
        {
            var item = await _service.GetItemAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] InventoryItem item)
        {
            var created = await _service.CreateItemAsync(item);
            if (created == null)
            {
                return Conflict("Item with same title or product id already exists.");
            }

            return CreatedAtAction(nameof(GetItems), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] InventoryItem item)
        {
            if (id != item.Id) return BadRequest();
            var updated = await _service.UpdateItemAsync(item);
            if (!updated)
                return Conflict("Item with same title or product id already exists.");
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            await _service.DeleteItemAsync(id);
            return NoContent();
        }

        [HttpGet("/player/{playerId}/inventory")]
        public async Task<IActionResult> GetPlayerInventory(Guid playerId)
        {
            var inv = await _service.GetPlayerInventoryAsync(playerId);
            return Ok(inv);
        }

        [HttpPut("/player/{playerId}/inventory")]
        public async Task<IActionResult> UpdatePlayerInventory(Guid playerId, [FromBody] List<PlayerInventoryItem> items)
        {
            await _service.UpdatePlayerInventoryAsync(playerId, items);
            return NoContent();
        }

        [HttpPost("/player/{playerId}/inventory/{itemId}/unpack")]
        public async Task<IActionResult> UnpackItem(Guid playerId, Guid itemId)
        {
            var success = await _service.UnpackItemAsync(playerId, itemId);
            if (!success) return BadRequest();
            return NoContent();
        }
    }
}
