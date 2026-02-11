using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/audit")]
[Authorize(Policy = "admin")]
public class AdminAuditController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminAuditController(AuthDbContext db) => _db = db;

    [RequirePermission("audit.read")]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? type,
        [FromQuery] string? userId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<SecurityEvent> filtered = _db.SecurityEvents.AsNoTracking()
            .OrderByDescending(e => e.CreatedAt);

        if (!string.IsNullOrWhiteSpace(type))
            filtered = filtered.Where(e => e.Type == type);
        if (!string.IsNullOrWhiteSpace(userId))
            filtered = filtered.Where(e => e.UserId == userId);
        if (from.HasValue)
            filtered = filtered.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue)
            filtered = filtered.Where(e => e.CreatedAt <= to.Value);

        var total = await filtered.CountAsync(ct);
        var items = await filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.Id, e.UserId, e.Email, e.Type, e.Detail, e.IpAddress, e.UserAgent, e.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse.Ok(new { items, total, page, pageSize }, HttpContext.TraceIdentifier));
    }
}
