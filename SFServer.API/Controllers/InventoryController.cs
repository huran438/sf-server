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
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var items = await _service.GetItemsAsync(projectId);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetItem(Guid id)
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var item = await _service.GetItemAsync(projectId, id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] InventoryItem item)
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var created = await _service.CreateItemAsync(projectId, item);
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
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var updated = await _service.UpdateItemAsync(projectId, item);
            if (!updated)
                return Conflict("Item with same title or product id already exists.");
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            await _service.DeleteItemAsync(projectId, id);
            return NoContent();
        }

        [HttpGet("/player/{playerId}/inventory")]
        public async Task<IActionResult> GetPlayerInventory(Guid playerId)
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            var inv = await _service.GetPlayerInventoryAsync(projectId, playerId);
            return Ok(inv);
        }

        [HttpPut("/player/{playerId}/inventory")]
        public async Task<IActionResult> UpdatePlayerInventory(Guid playerId, [FromBody] List<PlayerInventoryItem> items)
        {
            if (!Guid.TryParse(Request.Headers[Headers.PID], out var projectId))
                return BadRequest("ProjectId header required");
            await _service.UpdatePlayerInventoryAsync(projectId, playerId, items);
            return NoContent();
        }
    }
}
