using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Client.Session;
using SFServer.Shared.Server.UserProfile;
using SFServer.Shared.Server.Wallet;
using SFServer.Shared.Server.Inventory;
using SFServer.Shared.Server.Settings;
using SFServer.Shared.Server.Audit;
using SFServer.Shared.Server.Project;

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

        public DbSet<ProjectInfo> Projects { get; set; }

        public DbSet<UserDevice> UserDevices { get; set; }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<PlayerInventoryItem> PlayerInventoryItems { get; set; }

        public DbSet<UserSession> UserSessions { get; set; }

        public DbSet<ProjectSettings> ProjectSettings { get; set; }
        public DbSet<GlobalSettings> GlobalSettings { get; set; }

        public DbSet<AuditLogEntry> AuditLogs { get; set; }

        public DbSet<SFServer.Shared.Server.Admin.Administrator> Administrators { get; set; }


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