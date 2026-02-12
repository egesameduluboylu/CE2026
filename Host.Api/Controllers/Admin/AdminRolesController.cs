using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Contracts.Admin.Roles;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = "admin")]
public class AdminRolesController : ControllerBase
{
    private readonly AuthDbContext _db;

    public AdminRolesController(AuthDbContext db) => _db = db;

    // LIST roles
    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var items = await _db.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleListItem(r.Id, r.Name))
            .ToListAsync(ct);

        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    // LIST permissions catalog
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var items = await _db.Permissions
            .OrderBy(p => p.Key)
            .Select(p => new PermissionItem(p.Key, p.Description))
            .ToListAsync(ct);

        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    // ROLE detail (permissions)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null) return NotFound();

        var perms = await _db.RolePermissions
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.PermissionKey)
            .OrderBy(x => x)
            .ToArrayAsync(ct);

        return Ok(ApiResponse.Ok(new RoleDetailResponse(role.Id, role.Name, perms), HttpContext.TraceIdentifier));
    }

    // CREATE
    [RequirePermission("roles.write")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest req, CancellationToken ct)
    {
        var name = (req.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(ApiProblems.BadRequest("Role name required."));

        var exists = await _db.Roles.AnyAsync(r => r.Name == name, ct);
        if (exists) return Conflict(ApiProblems.Conflict("Role already exists."));

        var role = new AppRole { Name = name };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.Ok(new RoleListItem(role.Id, role.Name), HttpContext.TraceIdentifier));
    }

    // RENAME
    [RequirePermission("roles.write")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(Guid id, RenameRoleRequest req, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null) return NotFound();

        var name = (req.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(ApiProblems.BadRequest("Role name required."));

        var exists = await _db.Roles.AnyAsync(r => r.Id != id && r.Name == name, ct);
        if (exists) return Conflict(ApiProblems.Conflict("Role name already used."));

        role.Name = name;
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.Ok(new RoleListItem(role.Id, role.Name), HttpContext.TraceIdentifier));
    }

    // DELETE role
    [RequirePermission("roles.write")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null) return NotFound();

        var perms = await _db.RolePermissions.Where(x => x.RoleId == id).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(perms);
        var urs = await _db.UserRoles.Where(x => x.RoleId == id).ToListAsync(ct);
        _db.UserRoles.RemoveRange(urs);
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { deleted = true }, HttpContext.TraceIdentifier));
    }

    // SET role permissions (replace)
    [RequirePermission("roles.write")]
    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> SetRolePermissions(Guid id, UpdateRolePermissionsRequest req, CancellationToken ct)
    {
        var role = await _db.Roles.AnyAsync(r => r.Id == id, ct);
        if (!role) return NotFound();

        var desired = (req.Permissions ?? Array.Empty<string>())
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        // validate keys exist
        var valid = await _db.Permissions
            .Where(p => desired.Contains(p.Key))
            .Select(p => p.Key)
            .ToArrayAsync(ct);

        // remove old
        var old = await _db.RolePermissions.Where(x => x.RoleId == id).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(old);

        // add new
        _db.RolePermissions.AddRange(valid.Select(k => new AppRolePermission
        {
            RoleId = id,
            PermissionKey = k
        }));

        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.Ok(new { updated = valid.Length }, HttpContext.TraceIdentifier));
    }

    // GET user roles
    [RequirePermission("roles.read")]
    [HttpGet("~/api/admin/users/{userId:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken ct)
    {
     
        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToArrayAsync(ct);

        return Ok(ApiResponse.Ok(new UserRolesResponse(userId, roleIds), HttpContext.TraceIdentifier));
    }

    // SET user roles (replace)
    [RequirePermission("roles.write")]
    [HttpPut("~/api/admin/users/{userId:guid}/roles")]
    public async Task<IActionResult> SetUserRoles(Guid userId, SetUserRolesRequest req, CancellationToken ct)
    {
        var desired = (req.RoleIds).Distinct().ToArray();

        // validate roles exist
        var valid = await _db.Roles.Where(r => desired.Contains(r.Id)).Select(r => r.Id).ToArrayAsync(ct);

        // remove old
        var old = await _db.UserRoles.Where(x => x.UserId == userId).ToListAsync(ct);
        _db.UserRoles.RemoveRange(old);

        // add new
        _db.UserRoles.AddRange(valid.Select(rid => new AppUserRole
        {
            UserId = userId,
            RoleId = rid
        }));

        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.Ok(new { updated = valid.Length }, HttpContext.TraceIdentifier));
    }
}
