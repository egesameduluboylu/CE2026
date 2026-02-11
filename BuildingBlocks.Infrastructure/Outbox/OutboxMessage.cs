namespace BuildingBlocks.Infrastructure.Outbox;

public sealed record OutboxMessage(
    Guid Id,
    string Type,
    string Payload,
    DateTimeOffset OccurredAtUtc,
    string? CorrelationId = null);
