using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Client.Common;
using SFServer.Shared.Server.UserProfile;
using SFServer.Shared.Server.Wallet;

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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(p => p.Role)
                    .HasConversion<string>();
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