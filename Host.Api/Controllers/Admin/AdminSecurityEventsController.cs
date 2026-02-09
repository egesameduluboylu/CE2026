using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin")]
public class AdminSecurityEventsController : ControllerBase
{
    private readonly AuthDbContext _db;

    public AdminSecurityEventsController(AuthDbContext db) => _db = db;

    /// <summary>GET /api/admin/security-events?type=&from=&to=&page=1&pageSize=50</summary>
    [HttpGet("security-events")]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] string? type,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<SecurityEvent> query = _db.SecurityEvents.AsNoTracking().OrderByDescending(e => e.CreatedAt);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => e.Type == type);
        if (from.HasValue)
            query = query.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.CreatedAt <= to.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.Id,
                e.UserId,
                e.Email,
                e.Type,
                e.Detail,
                e.IpAddress,
                e.UserAgent,
                e.CreatedAt
            })
            .ToListAsync(ct);

        var data = new { items, total, page, pageSize };
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }
}
