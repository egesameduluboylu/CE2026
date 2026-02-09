namespace Modules.Identity.Infrastructure.Configuration;

public sealed class PasswordResetOptions
{
    public int TokenMinutes { get; set; } = 30;
    public string TokenPepper { get; set; } = "CHANGE_ME_LONG_RANDOM"; // env var ile ver
}
