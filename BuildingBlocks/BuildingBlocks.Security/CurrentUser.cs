using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Security;

public interface ICurrentUser
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? UserId =>
        _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _accessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public string? Email =>
        _accessor.HttpContext?.User?.Identity?.Name
        ?? _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _accessor.HttpContext?.User?.FindFirst("email")?.Value;

    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
