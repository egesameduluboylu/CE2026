using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddOutboxNoOp(this IServiceCollection services)
    {
        services.AddSingleton<IOutbox, NoOpOutbox>();
        return services;
    }
}
