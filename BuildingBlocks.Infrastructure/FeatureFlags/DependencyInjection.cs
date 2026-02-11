using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.FeatureFlags;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
    {
        // Provider chain (later: add DbFeatureFlagProvider, RemoteFeatureFlagProvider, etc.)
        services.AddSingleton<IFeatureFlagProvider>(_ => new ConfigurationFeatureFlagProvider(configuration));
        services.AddSingleton<IFeatureFlags, CompositeFeatureFlags>();
        return services;
    }

    public static IServiceCollection AddFeatureFlagProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IFeatureFlagProvider
    {
        services.AddSingleton<IFeatureFlagProvider, TProvider>();
        return services;
    }

    public static IServiceCollection AddFeatureFlagProviderFirst<TProvider>(this IServiceCollection services)
        where TProvider : class, IFeatureFlagProvider
    {
        services.Insert(0, ServiceDescriptor.Singleton<IFeatureFlagProvider, TProvider>());
        return services;
    }
}
