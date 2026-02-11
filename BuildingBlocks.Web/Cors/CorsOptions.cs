using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Web.Cors;

public sealed class CorsOptions
{
    public bool Enabled { get; init; } = false;

    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();

    [Required]
    [MinLength(1)]
    public string PolicyName { get; init; } = "default";
}
