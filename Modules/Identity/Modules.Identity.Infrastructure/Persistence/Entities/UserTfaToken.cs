using System;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class UserTfaToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// TOTP secret key (Base32 encoded)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Authenticator app name (e.g., "Platform MVP")
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email for authenticator
    /// </summary>
    public string AccountName { get; set; } = string.Empty;
    
    /// <summary>
    /// Is 2FA enabled for this user
    /// </summary>
    public bool IsEnabled { get; set; } = false;
    
    /// <summary>
    /// When 2FA was initially set up
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Last time 2FA was verified
    /// </summary>
    public DateTimeOffset? LastVerifiedAt { get; set; }
    
    /// <summary>
    /// Number of failed verification attempts
    /// </summary>
    public int FailedAttempts { get; set; } = 0;
    
    /// <summary>
    /// When verification attempts are locked out until
    /// </summary>
    public DateTimeOffset? LockedUntil { get; set; }

    // Navigation property
    public AppUser User { get; set; } = null!;
}
