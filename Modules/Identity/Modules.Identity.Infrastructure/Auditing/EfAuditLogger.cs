using BuildingBlocks.Abstractions.Auditing;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.Auditing;

public sealed class EfAuditLogger : IAuditLogger
{
    private readonly AuthDbContext _db;

    public EfAuditLogger(AuthDbContext db) => _db = db;

    public async Task TryWriteAsync(AuditEvent evt, CancellationToken ct = default)
    {
        try
        {
            _db.SecurityEvents.Add(new SecurityEvent
            {
                UserId = evt.UserId?.ToString(),
                Email = evt.Email,
                Type = evt.Type,
                Detail = evt.Detail,
                IpAddress = evt.IpAddress,
                UserAgent = evt.UserAgent,
                CreatedAt = evt.OccurredAtUtc ?? DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
        }
    }
}
