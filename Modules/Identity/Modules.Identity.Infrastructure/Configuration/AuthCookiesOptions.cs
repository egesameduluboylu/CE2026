namespace Modules.Identity.Infrastructure.Configuration;

public sealed class AuthCookiesOptions
{
    public string RefreshCookieName { get; init; } = "rt";
    public int RefreshCookieDays { get; init; } = 14;
    public string SameSite { get; init; } = "Lax";
}