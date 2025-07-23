using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.API.Services;
using SFServer.Shared.Server.Inventory;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("{projectId:guid}/[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _service;

        public InventoryController(InventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems(Guid projectId)
        {
            var items = await _service.GetItemsAsync(projectId);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetItem(Guid projectId, Guid id)
        {
            var item = await _service.GetItemAsync(projectId, id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(Guid projectId, [FromBody] InventoryItem item)
        {
            var created = await _service.CreateItemAsync(projectId, item);
            if (created == null)
            {
                return Conflict("Item with same title or product id already exists.");
            }

            return CreatedAtAction(nameof(GetItem), new { projectId, id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateItem(Guid projectId, Guid id, [FromBody] InventoryItem item)
        {
            if (id != item.Id) return BadRequest();
            var updated = await _service.UpdateItemAsync(projectId, item);
            if (!updated)
                return Conflict("Item with same title or product id already exists.");
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid projectId, Guid id)
        {
            await _service.DeleteItemAsync(projectId, id);
            return NoContent();
        }

        [HttpGet("player/{playerId}/inventory")]
        public async Task<IActionResult> GetPlayerInventory(Guid projectId, Guid playerId)
        {
            var inv = await _service.GetPlayerInventoryAsync(projectId, playerId);
            return Ok(inv);
        }

        [HttpPut("player/{playerId}/inventory")]
        public async Task<IActionResult> UpdatePlayerInventory(Guid projectId, Guid playerId, [FromBody] List<PlayerInventoryItem> items)
        {
            await _service.UpdatePlayerInventoryAsync(projectId, playerId, items);
            return NoContent();
        }
    }
}
