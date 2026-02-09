using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;

namespace Host.Api.Services;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceProvider services,
        IConfiguration config,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.GetValue<bool>("Cleanup:Enabled", true))
        {
            _logger.LogInformation("Refresh token cleanup is disabled.");
            return;
        }

        var intervalHours = _config.GetValue<int>("Cleanup:IntervalHours", 24);
        var retentionDays = _config.GetValue<int>("Cleanup:RetentionDays", 30);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoCleanupAsync(retentionDays, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token cleanup failed.");
            }

            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }

    private async Task DoCleanupAsync(int retentionDays, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        // Delete expired tokens
        var expired = await db.RefreshTokens
            .Where(t => t.ExpiresAt < DateTimeOffset.UtcNow)
            .ExecuteDeleteAsync(ct);

        // Delete old revoked tokens
        var oldRevoked = await db.RefreshTokens
            .Where(t => t.RevokedAt != null && t.RevokedAt < cutoff)
            .ExecuteDeleteAsync(ct);

        if (expired + oldRevoked > 0)
            _logger.LogInformation("Cleanup removed {Expired} expired and {Revoked} old revoked refresh tokens.", expired, oldRevoked);
    }
}
