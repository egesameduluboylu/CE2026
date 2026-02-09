namespace Modules.Identity.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public string Key { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 14;

}
