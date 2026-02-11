using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.Persistence;

public static class AuthDbContextSeed
{
    public static async Task SeedAsync(AuthDbContext db, CancellationToken ct = default)
    {
        // 1) Seed admin role
        var admin = await db.Roles.FirstOrDefaultAsync(x => x.Name == "admin", ct);
        if (admin is null)
        {
            admin = new AppRole { Name = "admin", Description = "System administrator" };
            db.Roles.Add(admin);
            await db.SaveChangesAsync(ct);
        }

        // 2) Seed permissions
        var existingPerms = await db.Permissions.Select(x => x.Key).ToListAsync(ct);
        var missingPerms = Permissions.All.Except(existingPerms).ToList();
        if (missingPerms.Count > 0)
        {
            db.Permissions.AddRange(missingPerms.Select(k => new AppPermission { Key = k }));
            await db.SaveChangesAsync(ct);
        }

        // 3) Give admin all permissions
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

        // 4) Seed admin user
        var adminUser = await db.Users.FirstOrDefaultAsync(x => x.Email == "admin@example.com", ct);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                Email = "admin@example.com",
                UserName = "admin",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                IsAdmin = true,
                CreatedBy = "system"
            };
            
            // Set password hash (you should use proper password hashing)
            adminUser.PasswordHash = "AQAAAAEAACcQAAAAEKqjXt1QyZ8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z"; // "Admin123!"
            
            db.Users.Add(adminUser);
            await db.SaveChangesAsync(ct);
        }

        // 5) Link admin user to admin role
        var userRole = await db.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == adminUser.Id && x.RoleId == admin.Id, ct);
        
        if (userRole is null)
        {
            db.UserRoles.Add(new AppUserRole
            {
                UserId = adminUser.Id,
                RoleId = admin.Id
            });
            await db.SaveChangesAsync(ct);
        }
    }
}
