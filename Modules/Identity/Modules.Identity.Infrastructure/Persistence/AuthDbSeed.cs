using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.Persistence;

public static class AuthDbSeed
{
    public static async Task SeedAsync(AuthDbContext db, CancellationToken ct = default)
    {
        // 1) admin role
        var admin = await db.Roles.FirstOrDefaultAsync(x => x.Name == "admin", ct);
        if (admin is null)
        {
            admin = new AppRole { Name = "admin", Description = "System administrator" };
            db.Roles.Add(admin);
            await db.SaveChangesAsync(ct);
        }

        // 2) permissions
        var existingPerms = await db.Permissions.Select(x => x.Key).ToListAsync(ct);
        var missingPerms = Permissions.All.Except(existingPerms).ToList();
        if (missingPerms.Count > 0)
        {
            db.Permissions.AddRange(missingPerms.Select(k => new AppPermission { Key = k }));
            await db.SaveChangesAsync(ct);
        }

        // 3) admin gets all perms
        var adminPerms = await db.RolePermissions
            .Where(x => x.RoleId == admin.Id)
            .Select(x => x.PermissionKey)
            .ToListAsync(ct);

        var addAdminPerms = Permissions.All.Except(adminPerms).ToList();
        if (addAdminPerms.Count > 0)
        {
            db.RolePermissions.AddRange(addAdminPerms.Select(k => new AppRolePermission
            {
                RoleId = admin.Id,
                PermissionKey = k
            }));
            await db.SaveChangesAsync(ct);
        }

        // 4) link IsAdmin users to admin role
        var adminUserIds = await db.Users.Where(u => u.IsAdmin).Select(u => u.Id).ToListAsync(ct);
        if (adminUserIds.Count > 0)
        {
            var alreadyLinked = await db.UserRoles
                .Where(ur => ur.RoleId == admin.Id)
                .Select(ur => ur.UserId)
                .ToListAsync(ct);

            var toLink = adminUserIds.Except(alreadyLinked).ToList();
            if (toLink.Count > 0)
            {
                db.UserRoles.AddRange(toLink.Select(uid => new AppUserRole { UserId = uid, RoleId = admin.Id }));
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
