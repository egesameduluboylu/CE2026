using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.FeatureFlags;

internal sealed class ConfigurationFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly IConfiguration _cfg;

    public ConfigurationFeatureFlagProvider(IConfiguration cfg) => _cfg = cfg;

    public bool? TryGet(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        // Distinguish between "missing" and "false":
        // - Missing -> null (let next provider decide)
        // - Present -> parsed bool
        var section = _cfg.GetSection($"FeatureFlags:{key}");
        if (!section.Exists()) return null;

        if (bool.TryParse(section.Value, out var b))
            return b;

        return null;
    }
}
