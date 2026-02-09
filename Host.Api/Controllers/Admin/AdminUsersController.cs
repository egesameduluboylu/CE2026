using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Admin;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUsersQuery _q;

    public AdminUsersController(IAdminUsersQuery q) => _q = q;
    [RequirePermission("users.read")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var data = await _q.GetUsersAsync(search, page, pageSize, ct);
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }
    [RequirePermission("users.read")]
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct = default)
    {
        var data = await _q.GetUserAsync(id, ct);
        if (data is null) return NotFound();
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }
    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/revoke-refresh-tokens")]
    public async Task<IActionResult> RevokeRefreshTokens(Guid id, CancellationToken ct = default)
    {
        var data = await _q.RevokeRefreshTokensAsync(id, ct);
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }
}
