namespace Modules.Identity.Contracts.Admin.Roles;

public sealed record RoleListItem(Guid Id, string Name);
public sealed record PermissionItem(string Key, string? Description);

public sealed record CreateRoleRequest(string Name);
public sealed record RenameRoleRequest(string Name);

public sealed record RoleDetailResponse(
    Guid Id,
    string Name,
    string[] Permissions
);

public sealed record UpdateRolePermissionsRequest(string[] Permissions);

public sealed record UserRolesResponse(Guid UserId, Guid[] RoleIds);
public sealed record SetUserRolesRequest(Guid[] RoleIds);
