namespace BuildingBlocks.Infrastructure.Outbox;

public interface IOutbox
{
    Task EnqueueAsync(OutboxMessage message, CancellationToken ct = default);
}
