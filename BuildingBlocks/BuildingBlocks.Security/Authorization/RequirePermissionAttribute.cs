using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security.Authorization;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = PermissionPolicy.Name(permission);
    }
}
