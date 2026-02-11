using System;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class UserBackupCode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Hashed backup code (SHA256)
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Has this backup code been used
    /// </summary>
    public bool IsUsed { get; set; } = false;
    
    /// <summary>
    /// When the backup code was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// When the backup code was used
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }
    
    /// <summary>
    /// IP address that used this backup code
    /// </summary>
    public string? UsedIpAddress { get; set; }
    
    /// <summary>
    /// User agent that used this backup code
    /// </summary>
    public string? UsedUserAgent { get; set; }

    // Navigation property
    public AppUser User { get; set; } = null!;
}
