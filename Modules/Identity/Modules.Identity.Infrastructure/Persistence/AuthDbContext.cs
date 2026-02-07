using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<AppUser>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.Email).HasMaxLength(320).IsRequired();

                e.Property(x => x.PasswordHash).IsRequired();
            });

            b.Entity<RefreshToken>(e =>
            {
                e.ToTable("RefreshTokens");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.TokenHash).IsUnique();
                e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();

                e.HasIndex(x => x.UserId);

                e.HasOne(x => x.User)
                    .WithMany() // AppUser'da RefreshTokens navigation yoksa böyle kalsın
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.ReplacedByToken)
                    .WithMany()
                    .HasForeignKey(x => x.ReplacedByTokenId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            });

        }
    }
}
