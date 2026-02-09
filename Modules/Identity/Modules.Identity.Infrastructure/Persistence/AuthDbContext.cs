using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence.Entities;
using System.Reflection.Emit;

namespace Modules.Identity.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<AppRole> Roles => Set<AppRole>();
        public DbSet<AppPermission> Permissions => Set<AppPermission>();
        public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();
        public DbSet<AppRolePermission> RolePermissions => Set<AppRolePermission>();


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

            b.Entity<SecurityEvent>(e =>
            {
                e.ToTable("SecurityEvents");
                e.HasKey(x => x.Id);
                e.Property(x => x.Type).HasMaxLength(64).IsRequired();
                e.Property(x => x.Email).HasMaxLength(320);
                e.Property(x => x.Detail).HasMaxLength(1024);
                e.Property(x => x.IpAddress).HasMaxLength(64);
                e.Property(x => x.UserAgent).HasMaxLength(512);
                e.HasIndex(x => x.CreatedAt);
                e.HasIndex(x => x.Type);
                e.HasIndex(x => x.UserId);
            });

            b.Entity<PasswordResetToken>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.TokenHash).IsUnique(false);
                b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
                b.Property(x => x.RequestedIp).HasMaxLength(128);
                b.Property(x => x.UserAgent).HasMaxLength(512);
            });

            b.Entity<AppRole>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Name).HasMaxLength(64).IsRequired();
                e.Property(x => x.Description).HasMaxLength(256);
            });

            b.Entity<AppPermission>(e =>
            {
                e.ToTable("Permissions");
                e.HasKey(x => x.Key);

                e.Property(x => x.Key).HasMaxLength(200).IsRequired();
                e.Property(x => x.Description).HasMaxLength(256);
            });

            b.Entity<AppUserRole>(e =>
            {
                e.ToTable("UserRoles");
                e.HasKey(x => new { x.UserId, x.RoleId });

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.RoleId);
            });

            b.Entity<AppRolePermission>(e =>
            {
                e.ToTable("RolePermissions");
                e.HasKey(x => new { x.RoleId, x.PermissionKey });

                e.Property(x => x.PermissionKey).HasMaxLength(200).IsRequired();

                e.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Permission)
                    .WithMany()
                    .HasForeignKey(x => x.PermissionKey)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.PermissionKey);
            });
        }
    }
}
