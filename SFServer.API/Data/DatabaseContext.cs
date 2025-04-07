using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Models.Inventory;
using SFServer.Shared.Models.UserProfile;
using SFServer.Shared.Models.Wallet;

namespace SFServer.API.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WalletItem> WalletItems { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Inventory> Inventories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(p => p.Role)
                    .HasConversion<string>();
            });

            // Configure Inventory -> User relationship
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.User)
                .WithOne()
                .HasForeignKey<Inventory>(i => i.UserId);

            // Configure Inventory -> Item relationship (One-to-many)
            modelBuilder.Entity<Inventory>()
                .HasMany(i => i.Items)
                .WithOne()
                .HasForeignKey("InventoryId"); // Optional:

            base.OnModelCreating(modelBuilder);
        }
    }
}