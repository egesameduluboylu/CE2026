using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;

namespace BuildingBlocks.Security.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly AuthDbContext _db;

    public PermissionAuthorizationHandler(AuthDbContext db) => _db = db;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdStr =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            context.User.FindFirstValue("sub") ??
            context.User.FindFirstValue("uid");

        if (!Guid.TryParse(userIdStr, out var userId))
            return;

        // IsAdmin shortcut
        var isAdmin = await _db.Users.Where(x => x.Id == userId).Select(x => x.IsAdmin).FirstOrDefaultAsync();
        if (isAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        // role -> permissions
        var has = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionKey)
            .AnyAsync(pk => pk == requirement.Permission);

        if (has)
            context.Succeed(requirement);
    }
}
