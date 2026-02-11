using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin")]
public class AdminStatsController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminStatsController(AuthDbContext db) => _db = db;

    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        var totalUsers = await _db.Users.CountAsync(ct);
        var totalTenants = await _db.Tenants.CountAsync(ct);
        var totalRoles = await _db.Roles.CountAsync(ct);
        var activeWebhooks = await _db.Webhooks.CountAsync(w => w.IsActive, ct);
        var totalApiKeys = await _db.ApiKeys.CountAsync(k => k.RevokedAt == null, ct);
        var totalFeatureFlags = await _db.FeatureFlags.CountAsync(ct);
        var enabledFeatures = await _db.FeatureFlags.CountAsync(f => f.Enabled, ct);
        var activeSessions = await _db.RefreshTokens.CountAsync(t => t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow, ct);

        var data = new
        {
            totalUsers,
            totalTenants,
            totalRoles,
            activeWebhooks,
            totalApiKeys,
            totalFeatureFlags,
            enabledFeatures,
            activeSessions
        };
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthSummary([FromServices] Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService hc, CancellationToken ct)
    {
        var report = await hc.CheckHealthAsync(ct);
        var data = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new { status = e.Value.Status.ToString(), duration = e.Value.Duration.TotalMilliseconds })
        };
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }
}
