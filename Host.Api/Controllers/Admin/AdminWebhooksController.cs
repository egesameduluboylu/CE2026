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
[Route("api/admin/webhooks")]
[Authorize(Policy = "admin")]
public class AdminWebhooksController : ControllerBase
{
    private readonly AuthDbContext _db;
    public AdminWebhooksController(AuthDbContext db) => _db = db;

    [RequirePermission("webhooks.read")]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _db.Webhooks.AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                w.Id, w.Name, w.Url, w.Events, w.IsActive, w.CreatedAt, w.DisabledAt
            })
            .ToListAsync(ct);
        return Ok(ApiResponse.Ok(items, HttpContext.TraceIdentifier));
    }

    [RequirePermission("webhooks.write")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWebhookDto dto, CancellationToken ct)
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var wh = new Webhook
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Url = dto.Url,
            Events = string.Join(",", dto.Events ?? Array.Empty<string>()),
            Secret = secret
        };
        _db.Webhooks.Add(wh);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = wh.Id, secret }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("webhooks.write")]
    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        var wh = await _db.Webhooks.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (wh is null) return NotFound();
        wh.IsActive = false;
        wh.DisabledAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { disabled = true }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("webhooks.write")]
    [HttpPost("{id:guid}/trigger")]
    public async Task<IActionResult> Trigger(Guid id, [FromBody] TriggerWebhookDto dto, CancellationToken ct)
    {
        var wh = await _db.Webhooks.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (wh is null) return NotFound();

        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            WebhookId = wh.Id,
            EventType = dto.EventType ?? "manual.test",
            PayloadUrl = wh.Url,
            Payload = dto.Payload ?? "{}",
            IsDelivered = false
        };
        _db.WebhookDeliveries.Add(delivery);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { deliveryId = delivery.Id }, HttpContext.TraceIdentifier));
    }
}

public record CreateWebhookDto(string Name, string Url, string[]? Events);
public record TriggerWebhookDto(string? EventType, string? Payload);
