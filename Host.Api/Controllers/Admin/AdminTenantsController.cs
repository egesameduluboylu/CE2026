using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/tenants")]
[Authorize(Policy = "admin")]
public class AdminTenantsController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminTenantsController(AuthDbContext db) => _db = db;

    [RequirePermission("tenants.read")]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _db.Tenants.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, t.IsActive, t.CreatedAt })
            .ToListAsync(ct);
        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("tenants.write")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto, CancellationToken ct)
    {
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = dto.Name.Trim() };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = tenant.Id }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("tenants.write")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(Guid id, [FromBody] RenameTenantDto dto, CancellationToken ct)
    {
        var t = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return NotFound();
        t.Name = dto.Name.Trim();
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { t.Id, t.Name }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("tenants.write")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetTenantStatusDto dto, CancellationToken ct)
    {
        var t = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return NotFound();
        t.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { t.Id, t.IsActive }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("tenants.write")]
    [HttpPut("{tenantId:guid}/plan")]
    public async Task<IActionResult> SetPlan(Guid tenantId, [FromBody] SetTenantPlanDto dto, CancellationToken ct)
    {
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (sub is null)
        {
            sub = new TenantSubscription { Id = Guid.NewGuid(), TenantId = tenantId, PlanId = dto.PlanId };
            _db.TenantSubscriptions.Add(sub);
        }
        else
        {
            sub.PlanId = dto.PlanId;
        }
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { tenantId, planId = dto.PlanId }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("tenants.read")]
    [HttpGet("usage")]
    public async Task<IActionResult> Usage(CancellationToken ct)
    {
        var tenants = await _db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(ct);
        var usage = tenants.ToDictionary(
            id => id.ToString(),
            _ => new { apiCalls = 0, webhooksCount = 0 }
        );
        return Ok(ApiResponse.Ok(usage, HttpContext.TraceIdentifier));
    }

    [RequirePermission("tenants.read")]
    [HttpGet("{tenantId:guid}/usage-chart")]
    public Task<IActionResult> UsageChart(Guid tenantId)
    {
        var data = Enumerable.Range(0, 30).Select(i =>
        {
            var date = DateTime.UtcNow.Date.AddDays(-29 + i);
            return new { date = date.ToString("yyyy-MM-dd"), apiCalls = 0, webhooks = 0 };
        }).ToArray();
        return Task.FromResult<IActionResult>(Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier)));
    }
}

public record CreateTenantDto(string Name);
public record RenameTenantDto(string Name);
public record SetTenantStatusDto(bool IsActive);
public record SetTenantPlanDto(Guid PlanId);
