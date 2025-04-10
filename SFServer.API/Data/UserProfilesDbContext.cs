using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Server.UserProfile;
using SFServer.Shared.Server.Wallet;

namespace SFServer.API.Data
{
    public class UserProfilesDbContext : DbContext
    {
        public UserProfilesDbContext(DbContextOptions<UserProfilesDbContext> options) : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WalletItem> WalletItems { get; set; }
        public DbSet<Currency> Currencies { get; set; }


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

            base.OnModelCreating(modelBuilder);
        }
    }
}