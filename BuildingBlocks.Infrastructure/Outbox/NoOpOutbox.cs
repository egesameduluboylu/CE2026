namespace BuildingBlocks.Infrastructure.Outbox;

internal sealed class NoOpOutbox : IOutbox
{
    public Task EnqueueAsync(OutboxMessage message, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
