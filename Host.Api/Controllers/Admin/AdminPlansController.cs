using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/plans")]
[Authorize(Policy = "admin")]
public class AdminPlansController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminPlansController(AuthDbContext db) => _db = db;

    [RequirePermission("billing.read")]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _db.Plans.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.MaxUsers, p.ApiCallsPerMinute, p.WebhooksCount, p.IsActive, p.CreatedAt })
            .ToListAsync(ct);
        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("billing.write")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanDto dto, CancellationToken ct)
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            MaxUsers = dto.MaxUsers,
            ApiCallsPerMinute = dto.ApiCallsPerMinute,
            WebhooksCount = dto.WebhooksCount,
            IsActive = true
        };
        _db.Plans.Add(plan);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = plan.Id }, HttpContext.TraceIdentifier));
    }
}

public record CreatePlanDto(string Name, int MaxUsers, int ApiCallsPerMinute, int WebhooksCount);
