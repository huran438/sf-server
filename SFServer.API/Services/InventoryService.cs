using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Inventory;

namespace SFServer.API.Services;

public class InventoryService
{
    private readonly DatabseContext _db;

    public InventoryService(DatabseContext db)
    {
        _db = db;
    }

    public Task<List<InventoryItem>> GetItemsAsync() => _db.InventoryItems.ToListAsync();

    public Task<InventoryItem?> GetItemAsync(Guid id) => _db.InventoryItems.FindAsync(id).AsTask();

    public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
    {
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task UpdateItemAsync(InventoryItem item)
    {
        _db.InventoryItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(Guid id)
    {
        var existing = await _db.InventoryItems.FindAsync(id);
        if (existing != null)
        {
            _db.InventoryItems.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }

    public Task<List<PlayerInventoryItem>> GetPlayerInventoryAsync(Guid playerId)
        => _db.PlayerInventoryItems.Where(p => p.UserId == playerId).ToListAsync();

    public async Task UpdatePlayerInventoryAsync(Guid playerId, List<PlayerInventoryItem> items)
    {
        var existing = _db.PlayerInventoryItems.Where(p => p.UserId == playerId);
        _db.PlayerInventoryItems.RemoveRange(existing);
        foreach (var item in items)
        {
            item.Id = Guid.NewGuid();
            item.UserId = playerId;
            _db.PlayerInventoryItems.Add(item);
        }
        await _db.SaveChangesAsync();
    }
}
