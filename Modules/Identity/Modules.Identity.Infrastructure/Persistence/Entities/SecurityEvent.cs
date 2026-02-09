namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class SecurityEvent
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string Type { get; set; } = ""; // LOGIN_SUCCESS, LOGIN_FAIL, LOCKOUT, REFRESH_REUSED, REFRESH_ROTATED, LOGOUT, etc.
    public string? Detail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
