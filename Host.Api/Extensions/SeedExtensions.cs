using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;

namespace Host.Api.Extensions;

public static class SeedExtensions
{
    public static async Task SeedIdentityAsync(this WebApplication app, CancellationToken ct = default)
    {
        using var scope = app.Services.CreateScope();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var pw = scope.ServiceProvider.GetRequiredService<PasswordService>();

        var seedEmail = cfg["Seed:AdminEmail"];
        var seedPass = cfg["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPass))
            return;

        seedEmail = seedEmail.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(x => x.Email == seedEmail, ct);
        if (exists) return;

        db.Users.Add(new AppUser
        {
            Email = seedEmail,
            PasswordHash = pw.Hash(seedPass),
            IsAdmin = true
        });

        await db.SaveChangesAsync(ct);
    }
}
