namespace BuildingBlocks.Abstractions.Auditing;

public interface IAuditLogger
{
    Task TryWriteAsync(AuditEvent evt, CancellationToken ct = default);
}
