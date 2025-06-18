using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Client.Common;
using SFServer.Shared.Server.UserProfile;
using SFServer.Shared.Server.Wallet;
using SFServer.Shared.Server.Inventory;

namespace SFServer.API.Data
{
    public class DatabseContext : DbContext
    {
        public DatabseContext(DbContextOptions<DatabseContext> options) : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WalletItem> WalletItems { get; set; }
        public DbSet<Currency> Currencies { get; set; }

        public DbSet<UserDevice> UserDevices { get; set; }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<PlayerInventoryItem> PlayerInventoryItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(p => p.Role)
                    .HasConversion<string>();
            });

            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.Property(p => p.Type)
                    .HasConversion<string>();
                entity.Property(p => p.Rarity)
                    .HasConversion<string>();
                entity.Property(p => p.Prices)
                    .HasColumnType("jsonb");
            });

            modelBuilder.Entity<PlayerInventoryItem>(entity =>
            {
                entity.HasOne<InventoryItem>()
                    .WithMany()
                    .HasForeignKey(p => p.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<UserProfile>()
                    .WithMany(u => u.PlayerInventory)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder
                .HasSequence<int>("UserProfileIndex", schema: "dbo")
                .StartsAt(1)
                .IncrementsBy(1);
            
            modelBuilder.Entity<UserProfile>()
                .Property(u => u.Index)
                .UseIdentityColumn(); 

            base.OnModelCreating(modelBuilder);
        }
    }
}