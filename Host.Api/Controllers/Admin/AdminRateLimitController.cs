using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/rate-limit")]
[Authorize(Policy = "admin")]
public class AdminRateLimitController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminRateLimitController(AuthDbContext db) => _db = db;

    [RequirePermission("rate_limit.read")]
    [HttpGet("quotas")]
    public async Task<IActionResult> ListQuotas(CancellationToken ct)
    {
        var items = await _db.RateLimitQuotas.AsNoTracking()
            .OrderBy(q => q.TenantId).ThenBy(q => q.EndpointKey)
            .Select(q => new
            {
                q.Id, q.TenantId, q.EndpointKey, q.PermitLimit, q.WindowSeconds, q.Burst, q.UserId, q.CreatedAt, q.UpdatedAt
            })
            .ToListAsync(ct);
        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("rate_limit.write")]
    [HttpPost("quotas")]
    public async Task<IActionResult> CreateQuota([FromBody] CreateQuotaDto dto, CancellationToken ct)
    {
        var quota = new RateLimitQuota
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            EndpointKey = dto.EndpointKey,
            PermitLimit = dto.PermitLimit,
            WindowSeconds = dto.WindowSeconds,
            Burst = dto.Burst,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.RateLimitQuotas.Add(quota);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = quota.Id }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("rate_limit.write")]
    [HttpPut("quotas/{id:guid}")]
    public async Task<IActionResult> UpdateQuota(Guid id, [FromBody] UpdateQuotaDto dto, CancellationToken ct)
    {
        var quota = await _db.RateLimitQuotas.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (quota is null) return NotFound();
        quota.PermitLimit = dto.PermitLimit;
        quota.WindowSeconds = dto.WindowSeconds;
        quota.Burst = dto.Burst;
        quota.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { quota.Id }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("rate_limit.write")]
    [HttpDelete("quotas/{id:guid}")]
    public async Task<IActionResult> DeleteQuota(Guid id, CancellationToken ct)
    {
        var quota = await _db.RateLimitQuotas.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (quota is null) return NotFound();
        _db.RateLimitQuotas.Remove(quota);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { deleted = true }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("rate_limit.read")]
    [HttpGet("tenants/{tenantId:guid}/usage")]
    public Task<IActionResult> TenantUsage(Guid tenantId)
    {
        var data = Array.Empty<object>();
        return Task.FromResult<IActionResult>(Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier)));
    }
}

public record CreateQuotaDto(Guid TenantId, string EndpointKey, long PermitLimit, long WindowSeconds, long Burst, string? UserId);
public record UpdateQuotaDto(long PermitLimit, long WindowSeconds, long Burst);
