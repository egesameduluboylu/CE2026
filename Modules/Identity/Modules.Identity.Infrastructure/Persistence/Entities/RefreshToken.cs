using Modules.Identity.Infrastructure.Persistence.Entities;
using System.ComponentModel.DataAnnotations;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string TokenHash { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public Guid? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }

    [Timestamp]                 // ✅ EF concurrency token
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public bool IsActive(DateTimeOffset now) => RevokedAt == null && ExpiresAt > now;

}
