using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/feature-flags")]
[Authorize(Policy = "admin")]
public class AdminFeatureFlagsController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminFeatureFlagsController(AuthDbContext db) => _db = db;

    [RequirePermission("flags.read")]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _db.FeatureFlags.AsNoTracking()
            .OrderBy(f => f.Key)
            .Select(f => new { f.Id, f.Key, f.Enabled, f.Description, f.CreatedAt, f.UpdatedAt })
            .ToListAsync(ct);
        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("flags.write")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureFlagDto dto, CancellationToken ct)
    {
        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Key = dto.Key.Trim(),
            Enabled = dto.Enabled,
            Description = dto.Description
        };
        _db.FeatureFlags.Add(flag);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = flag.Id }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("flags.write")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeatureFlagDto dto, CancellationToken ct)
    {
        var flag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (flag is null) return NotFound();
        flag.Enabled = dto.Enabled;
        flag.Description = dto.Description ?? flag.Description;
        flag.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { flag.Id, flag.Key, flag.Enabled }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("flags.write")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var flag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (flag is null) return NotFound();
        _db.FeatureFlags.Remove(flag);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { deleted = true }, HttpContext.TraceIdentifier));
    }
}

public record CreateFeatureFlagDto(string Key, bool Enabled, string? Description);
public record UpdateFeatureFlagDto(bool Enabled, string? Description);
