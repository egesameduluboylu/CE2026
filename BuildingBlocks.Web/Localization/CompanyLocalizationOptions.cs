using System.Globalization;

namespace BuildingBlocks.Web.Localization;

public sealed class CompanyLocalizationOptions
{
    public string ResourcesPath { get; set; } = "Resources";

    public CultureInfo DefaultCulture { get; set; } = new("tr-TR");

    public IReadOnlyList<CultureInfo> SupportedCultures { get; set; } = new List<CultureInfo>
    {
        new("tr-TR"),
        new("en-GB"),
        new("en-US"),
        // kısa kodlar (React bazen tr/en gönderir)
        new("tr"),
        new("en")
    };

    // Provider sırası:
    // Cookie (opsiyonel) -> Accept-Language (React) -> (opsiyonel QueryString)
    public bool EnableCookieProvider { get; set; } = true;
    public bool EnableQueryStringProvider { get; set; } = false;
}
