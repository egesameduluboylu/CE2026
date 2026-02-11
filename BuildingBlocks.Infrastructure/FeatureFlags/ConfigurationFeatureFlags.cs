using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.FeatureFlags;

internal sealed class ConfigurationFeatureFlags : IFeatureFlags
{
    private readonly IConfiguration _cfg;

    public ConfigurationFeatureFlags(IConfiguration cfg) => _cfg = cfg;

    public bool IsEnabled(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return _cfg.GetValue<bool>($"FeatureFlags:{key}");
    }
}
