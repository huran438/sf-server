using Microsoft.EntityFrameworkCore;
using SFServer.API.Data;
using SFServer.Shared.Server.Inventory;
using SFServer.Shared.Server.Purchases;
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
            => _db.InventoryItems
                .Where(i => i.ProjectId == projectId)
                .Include(i => i.Drops)
                .ToListAsync();

        public Task<InventoryItem> GetItemAsync(Guid projectId, Guid id)
            => _db.InventoryItems
                .Include(i => i.Drops)
                .FirstOrDefaultAsync(i => i.Id == id && i.ProjectId == projectId);

        public async Task<InventoryItem> CreateItemAsync(Guid projectId, InventoryItem item)
        {
            if (await _db.InventoryItems.AnyAsync(i => i.Title == item.Title))
            {
                return null;
            }

            item.Id = Guid.CreateVersion7();
            item.ProjectId = projectId;
            var drops = item.Drops ?? new List<InventoryItemDrop>();
            drops = drops
                .Where(d => d.Amount != 0)
                .GroupBy(d => new { d.Type, d.TargetId })
                .Select(g => new InventoryItemDrop
                {
                    Id = Guid.CreateVersion7(),
                    ItemId = item.Id,
                    Type = g.Key.Type,
                    TargetId = g.Key.TargetId,
                    Amount = g.Sum(x => x.Amount)
                }).ToList();
            item.Drops = drops;

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

            var existing = await _db.InventoryItems.Include(i => i.Drops).FirstOrDefaultAsync(i => i.Id == item.Id && i.ProjectId == projectId);
            if (existing == null)
                return false;

            _db.Entry(existing).CurrentValues.SetValues(item);

            var drops = item.Drops ?? new List<InventoryItemDrop>();
            var dedup = drops
                .Where(d => d.Amount != 0)
                .GroupBy(d => new { d.Type, d.TargetId })
                .Select(g => new InventoryItemDrop
                {
                    Id = Guid.CreateVersion7(),
                    ItemId = item.Id,
                    Type = g.Key.Type,
                    TargetId = g.Key.TargetId,
                    Amount = g.Sum(x => x.Amount)
                }).ToList();

            var existingDrops = _db.InventoryItemDrops.Where(d => d.ItemId == item.Id);
            _db.InventoryItemDrops.RemoveRange(existingDrops);
            foreach (var d in dedup)
                _db.InventoryItemDrops.Add(d);

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

        private async Task ApplyDropsInternalAsync(IEnumerable<InventoryItemDrop> drops, Guid userId, int multiplier)
        {
            foreach (var drop in drops)
            {
                var amount = drop.Amount * multiplier;
                if (drop.Type == ProductDropType.Item)
                {
                    var targetItem = await _db.InventoryItems
                        .Include(i => i.Drops)
                        .FirstOrDefaultAsync(i => i.Id == drop.TargetId);
                    if (targetItem != null && targetItem.Drops.Count > 0 && targetItem.UnpackOnDrop)
                    {
                        await ApplyDropsInternalAsync(targetItem.Drops, userId, amount);
                        continue;
                    }

                    var inv = await _db.PlayerInventoryItems.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == drop.TargetId);
                    if (inv == null)
                    {
                        inv = new PlayerInventoryItem
                        {
                            Id = Guid.CreateVersion7(),
                            UserId = userId,
                            ItemId = drop.TargetId,
                            Amount = amount
                        };
                        _db.PlayerInventoryItems.Add(inv);
                    }
                    else
                    {
                        inv.Amount += amount;
                    }
                }
                else if (drop.Type == ProductDropType.Currency)
                {
                    var wallet = await _db.WalletItems.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyId == drop.TargetId);
                    if (wallet == null)
                    {
                        wallet = new WalletItem
                        {
                            Id = Guid.CreateVersion7(),
                            UserId = userId,
                            CurrencyId = drop.TargetId,
                            Amount = amount
                        };
                        _db.WalletItems.Add(wallet);
                    }
                    else
                    {
                        wallet.Amount += amount;
                    }
                }
            }
        }

        public async Task ApplyItemDropsAsync(Guid itemId, Guid userId, int multiplier)
        {
            var drops = await _db.InventoryItemDrops.Where(d => d.ItemId == itemId && d.Amount != 0).ToListAsync();
            await ApplyDropsInternalAsync(drops, userId, multiplier);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> UnpackItemAsync(Guid projectId, Guid userId, Guid itemId, int amount)
        {
            if (amount <= 0) return false;
            var item = await _db.InventoryItems
                .Include(i => i.Drops)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ProjectId == projectId);
            if (item == null || item.Drops.Count == 0)
                return false;

            var inv = await _db.PlayerInventoryItems.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == itemId);
            if (inv == null || inv.Amount < amount)
                return false;

            inv.Amount -= amount;
            if (inv.Amount == 0)
                _db.PlayerInventoryItems.Remove(inv);

            await ApplyDropsInternalAsync(item.Drops, userId, amount);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
