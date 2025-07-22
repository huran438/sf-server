using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Inventory;
using SFServer.Shared.Server.Wallet;

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
                    i.Title == item.Title ||
                    (i.ProductId != null && i.ProductId == item.ProductId)))
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
                    (i.Title == item.Title ||
                     (i.ProductId != null && i.ProductId == item.ProductId))))
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

            foreach (var entry in items)
            {
                if (entry.Amount <= 0) continue;
                await AddItemsToPlayerAsync(playerId, entry.ItemId, entry.Amount);
            }

            await _db.SaveChangesAsync();
            await AutoUnpackPlayerInventoryAsync(playerId);
        }

        public async Task AddItemsToPlayerAsync(Guid playerId, Guid itemId, int amount)
        {
            var definition = await _db.InventoryItems.FindAsync(itemId);
            if (definition == null || amount <= 0) return;

            if (definition.AutoUnpack && definition.Drop.Count > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    await ApplyDropAsync(playerId, definition.Drop);
                }
                return;
            }

            var existing = await _db.PlayerInventoryItems.FirstOrDefaultAsync(p => p.UserId == playerId && p.ItemId == itemId);
            if (existing == null)
            {
                existing = new PlayerInventoryItem
                {
                    Id = Guid.CreateVersion7(),
                    UserId = playerId,
                    ItemId = itemId,
                    Amount = amount
                };
                _db.PlayerInventoryItems.Add(existing);
            }
            else
            {
                existing.Amount += amount;
            }
        }

        private async Task ApplyDropAsync(Guid playerId, List<InventoryDropEntry> drop)
        {
            foreach (var d in drop)
            {
                if (d.ItemId.HasValue)
                {
                    await AddItemsToPlayerAsync(playerId, d.ItemId.Value, d.Amount);
                }
                else if (d.CurrencyId.HasValue)
                {
                    var wallet = await _db.WalletItems.FirstOrDefaultAsync(w => w.UserId == playerId && w.CurrencyId == d.CurrencyId.Value);
                    if (wallet == null)
                    {
                        var currency = await _db.Currencies.FindAsync(d.CurrencyId.Value);
                        if (currency == null) continue;
                        wallet = new WalletItem
                        {
                            Id = Guid.CreateVersion7(),
                            UserId = playerId,
                            CurrencyId = d.CurrencyId.Value,
                            Currency = currency,
                            Amount = 0
                        };
                        _db.WalletItems.Add(wallet);
                    }
                    wallet.Amount += d.Amount;
                }
            }
        }

        public async Task<bool> UnpackItemAsync(Guid playerId, Guid itemId)
        {
            var invItem = await _db.PlayerInventoryItems.FirstOrDefaultAsync(p => p.UserId == playerId && p.ItemId == itemId);
            if (invItem == null || invItem.Amount <= 0) return false;

            var definition = await _db.InventoryItems.FindAsync(itemId);
            if (definition == null || definition.Drop.Count == 0) return false;

            invItem.Amount -= 1;
            if (invItem.Amount <= 0)
                _db.PlayerInventoryItems.Remove(invItem);

            await ApplyDropAsync(playerId, definition.Drop);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task AutoUnpackPlayerInventoryAsync(Guid playerId)
        {
            var items = await _db.PlayerInventoryItems.Where(p => p.UserId == playerId).ToListAsync();
            foreach (var inv in items)
            {
                var def = await _db.InventoryItems.FindAsync(inv.ItemId);
                if (def != null && def.AutoUnpack && def.Drop.Count > 0)
                {
                    for (int i = 0; i < inv.Amount; i++)
                    {
                        await ApplyDropAsync(playerId, def.Drop);
                    }
                    _db.PlayerInventoryItems.Remove(inv);
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
