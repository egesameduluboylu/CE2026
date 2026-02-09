using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin")]
public class AdminHealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheck;

    public AdminHealthController(HealthCheckService healthCheck) => _healthCheck = healthCheck;

    /// <summary>GET /api/admin/health/ready — includes DB check</summary>
    [HttpGet("health/ready")]
    public async Task<IActionResult> Ready(CancellationToken ct = default)
    {
        var report = await _healthCheck.CheckHealthAsync(ct);
        var status = report.Status == HealthStatus.Healthy ? 200 : 503;
        var data = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
        };
        return StatusCode(status, ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }

    /// <summary>GET /api/admin/health/live — liveness probe</summary>
    [HttpGet("health/live")]
    public IActionResult Live()
    {
        return Ok(ApiResponse.Ok(new { status = "alive" }, HttpContext.TraceIdentifier));
    }
}
