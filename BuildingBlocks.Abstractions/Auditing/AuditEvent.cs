namespace BuildingBlocks.Abstractions.Auditing;

public sealed record AuditEvent(
    string Type,
    Guid? UserId,
    string? Email,
    string? Detail,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset? OccurredAtUtc = null);
