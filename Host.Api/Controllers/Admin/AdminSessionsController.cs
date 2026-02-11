using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/sessions")]
[Authorize(Policy = "admin")]
public class AdminSessionsController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminSessionsController(AuthDbContext db) => _db = db;

    [RequirePermission("sessions.read")]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var now = DateTimeOffset.UtcNow;

        var query = _db.RefreshTokens.AsNoTracking()
            .Where(t => t.RevokedAt == null && t.ExpiresAt > now);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            if (Guid.TryParse(userId, out var userGuid))
                query = query.Where(t => t.UserId == userGuid);
            else
                query = query.Where(t => t.UserId.ToString() == userId);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.ExpiresAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                id = t.Id,
                userId = t.UserId,
                email = t.User != null ? t.User.Email : null,
                expiresAt = t.ExpiresAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("sessions.write")]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (token is null) return NotFound();
        token.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { revoked = true }, HttpContext.TraceIdentifier));
    }
}
