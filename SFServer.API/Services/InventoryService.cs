using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Inventory;

namespace SFServer.API.Services
{
    public class InventoryService
    {
        private readonly DatabseContext _db;

        public InventoryService(DatabseContext db)
        {
            _db = db;
        }

        public Task<List<InventoryItem>> GetItemsAsync(Guid projectId)
            => _db.InventoryItems.Where(i => i.ProjectId == projectId).ToListAsync();

        public Task<InventoryItem> GetItemAsync(Guid projectId, Guid id)
            => _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id && i.ProjectId == projectId);

        public async Task<InventoryItem> CreateItemAsync(Guid projectId, InventoryItem item)
        {
            if (await _db.InventoryItems.AnyAsync(i =>
                    i.Title == item.Title))
            {
                return null;
            }

            item.ProjectId = projectId;
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateItemAsync(Guid projectId, InventoryItem item)
        {
            if (await _db.InventoryItems.AnyAsync(i =>
                    i.Id != item.Id &&
                    i.Title == item.Title))
            {
                return false;
            }

            var existing = await _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == item.Id && i.ProjectId == projectId);
            if (existing == null)
                return false;

            _db.Entry(existing).CurrentValues.SetValues(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task DeleteItemAsync(Guid projectId, Guid id)
        {
            var existing = await _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id && i.ProjectId == projectId);
            if (existing != null)
            {
                _db.InventoryItems.Remove(existing);
                await _db.SaveChangesAsync();
            }
        }

        public Task<List<PlayerInventoryItem>> GetPlayerInventoryAsync(Guid projectId, Guid playerId)
            => _db.PlayerInventoryItems.Where(p => p.UserId == playerId).ToListAsync();

        public async Task UpdatePlayerInventoryAsync(Guid projectId, Guid playerId, List<PlayerInventoryItem> items)
        {
            var existing = _db.PlayerInventoryItems.Where(p => p.UserId == playerId);
            _db.PlayerInventoryItems.RemoveRange(existing);

            var grouped = items
                .Where(i => i.Amount > 0)
                .GroupBy(i => i.ItemId)
                .Select(g => new PlayerInventoryItem
                {
                    ItemId = g.Key,
                    Amount = g.Sum(x => x.Amount)
                });

            foreach (var item in grouped)
            {
                item.Id = Guid.CreateVersion7();
                item.UserId = playerId;
                _db.PlayerInventoryItems.Add(item);
            }

            await _db.SaveChangesAsync();
        }
    }
}
