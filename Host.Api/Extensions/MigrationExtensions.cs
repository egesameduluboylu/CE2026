using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;

namespace Host.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken ct = default)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            logger.LogInformation("Applying AuthDbContext migrations...");
            await db.Database.MigrateAsync(ct);
            logger.LogInformation("Migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database migration failed.");
            throw;
        }
    }
}
