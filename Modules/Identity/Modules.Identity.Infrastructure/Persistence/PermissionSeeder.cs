using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Persistence.Entities;
using System.Data;
using System.Security;

namespace Modules.Identity.Infrastructure.Persistence;

public static class PermissionSeeder
{
    public static async Task SeedAsync(AuthDbContext db, CancellationToken ct = default)
    {
        // 1️⃣ Permissions
        var existing = await db.Permissions
            .Select(x => x.Key)
            .ToListAsync(ct);

        var missing = Permissions.All
            .Where(p => !existing.Contains(p))
            .Select(p => new AppPermission
            {
                Key = p,
                Description = p
            });

        if (missing.Any())
        {
            db.Permissions.AddRange(missing);
            await db.SaveChangesAsync(ct);
        }

        // 2️⃣ Admin role
        var adminRole = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == "admin", ct);

        if (adminRole == null)
        {
            adminRole = new AppRole
            {
                Name = "admin"
            };

            db.Roles.Add(adminRole);
            await db.SaveChangesAsync(ct);
        }

        // 3️⃣ RolePermissions
        var rolePerms = await db.RolePermissions
            .Where(x => x.RoleId == adminRole.Id)
            .Select(x => x.PermissionKey)
            .ToListAsync(ct);

        var missingRolePerms = Permissions.All
            .Where(p => !rolePerms.Contains(p))
            .Select(p => new AppRolePermission
            {
                RoleId = adminRole.Id,
                PermissionKey = p
            });

        if (missingRolePerms.Any())
        {
            db.RolePermissions.AddRange(missingRolePerms);
            await db.SaveChangesAsync(ct);
        }

        // 4️⃣ Eğer sistemde IsAdmin=true user varsa role ata
        var admins = await db.Users
            .Where(u => u.IsAdmin)
            .ToListAsync(ct);

        foreach (var user in admins)
        {
            var exists = await db.UserRoles
                .AnyAsync(x => x.UserId == user.Id && x.RoleId == adminRole.Id, ct);

            if (!exists)
            {
                db.UserRoles.Add(new AppUserRole
                {
                    UserId = user.Id,
                    RoleId = adminRole.Id
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
