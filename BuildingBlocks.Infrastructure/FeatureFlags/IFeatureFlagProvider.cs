namespace BuildingBlocks.Infrastructure.FeatureFlags;

public interface IFeatureFlagProvider
{
    /// <summary>
    /// Returns a flag value if the provider can resolve it; otherwise null.
    /// </summary>
    bool? TryGet(string key);
}
