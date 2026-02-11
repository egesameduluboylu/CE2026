namespace BuildingBlocks.Infrastructure.FeatureFlags;

internal sealed class CompositeFeatureFlags : IFeatureFlags
{
    private readonly IEnumerable<IFeatureFlagProvider> _providers;

    public CompositeFeatureFlags(IEnumerable<IFeatureFlagProvider> providers) => _providers = providers;

    public bool IsEnabled(string key)
    {
        foreach (var p in _providers)
        {
            var v = p.TryGet(key);
            if (v.HasValue)
                return v.Value;
        }

        return false;
    }
}
