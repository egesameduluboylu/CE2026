namespace Modules.Identity.Infrastructure.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class PasswordResetOptions
{
    [Range(1, 1440)]
    public int TokenMinutes { get; set; } = 30;

    [Required]
    [MinLength(16)]
    public string TokenPepper { get; set; } = "CHANGE_ME_LONG_RANDOM"; // env var ile ver
}
