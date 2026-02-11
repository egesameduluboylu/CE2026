namespace BuildingBlocks.Infrastructure.FeatureFlags;

public interface IFeatureFlags
{
    bool IsEnabled(string key);
}
