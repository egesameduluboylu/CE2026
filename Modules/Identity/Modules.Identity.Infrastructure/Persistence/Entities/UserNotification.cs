namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Notification type: info, success, warning, error
    /// </summary>
    public string Type { get; set; } = "info";
    
    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Notification message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Action URL (optional)
    /// </summary>
    public string? ActionUrl { get; set; }
    
    /// <summary>
    /// Action button text (optional)
    /// </summary>
    public string? ActionText { get; set; }
    
    /// <summary>
    /// Is notification read
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// When notification was read
    /// </summary>
    public DateTimeOffset? ReadAt { get; set; }
    
    /// <summary>
    /// When notification expires (optional)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
    
    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // BaseEntity properties
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public AppUser User { get; set; } = null!;
}
