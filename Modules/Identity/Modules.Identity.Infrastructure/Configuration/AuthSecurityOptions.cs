namespace Modules.Identity.Infrastructure.Configuration;

public sealed class AuthSecurityOptions
{
    public int MaxFailedLogins { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 10;
}