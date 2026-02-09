namespace Modules.Identity.Infrastructure.Persistence.Entities;

public sealed class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = default!; // SHA256(base64urltoken + pepper)
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public string? RequestedIp { get; set; }
    public string? UserAgent { get; set; }
}
