using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Persistence.Configurations;

namespace Modules.Identity.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public AuthDbContext(DbContextOptions<AuthDbContext> options, IServiceProvider serviceProvider) : base(options) 
    {
        _serviceProvider = serviceProvider;
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<AppPermission> Permissions => Set<AppPermission>();
    public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();
    public DbSet<AppRolePermission> RolePermissions => Set<AppRolePermission>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<RateLimitQuota> RateLimitQuotas => Set<RateLimitQuota>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserTfaToken> UserTfaTokens => Set<UserTfaToken>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
    public DbSet<UserBackupCode> UserBackupCodes => Set<UserBackupCode>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();
    public DbSet<BackgroundJobLog> BackgroundJobLogs => Set<BackgroundJobLog>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<I18nResource> I18nResources => Set<I18nResource>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new I18nResourceConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // TODO: Add interceptor when UserContext is properly configured
        // var userContext = _serviceProvider.GetService<IUserContext>();
        // optionsBuilder.AddInterceptors(new AuditableEntityInterceptor(userContext));
    }
}
