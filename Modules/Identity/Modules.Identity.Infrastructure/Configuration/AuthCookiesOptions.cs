namespace Modules.Identity.Infrastructure.Configuration;

public sealed class AuthCookiesOptions
{
    public string RefreshCookieName { get; init; } = "rt";
}