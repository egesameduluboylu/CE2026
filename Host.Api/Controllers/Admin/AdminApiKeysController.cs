using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using System.Security.Cryptography;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/api-keys")]
[Authorize(Policy = "admin")]
public class AdminApiKeysController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminApiKeysController(AuthDbContext db) => _db = db;

    [RequirePermission("api_keys.read")]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _db.ApiKeys.AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new { k.Id, k.Name, k.Prefix, k.Scopes, k.ExpiresAt, k.RevokedAt, k.LastUsedAt, k.CreatedAt })
            .ToListAsync(ct);
        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("api_keys.write")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto, CancellationToken ct)
    {
        var rawSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var prefix = rawSecret[..8];
        var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawSecret)));

        var key = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Prefix = prefix,
            SecretHash = hash,
            Scopes = dto.Scopes ?? "",
            ExpiresAt = dto.ExpiresAt
        };
        _db.ApiKeys.Add(key);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = key.Id, secret = rawSecret }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("api_keys.write")]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (key is null) return NotFound();
        key.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { revoked = true }, HttpContext.TraceIdentifier));
    }
}

public record CreateApiKeyDto(string Name, string? Scopes, DateTimeOffset? ExpiresAt);
