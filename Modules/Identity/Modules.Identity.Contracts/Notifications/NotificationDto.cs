namespace Modules.Identity.Contracts.Notifications;

/// <summary>
/// Notification types
/// </summary>
public static class NotificationType
{
    public const string Info = "info";
    public const string Success = "success";
    public const string Warning = "warning";
    public const string Error = "error";
}

/// <summary>
/// Notification response for API
/// </summary>
public sealed record NotificationResponse(
    string Id,
    string Type,
    string Title,
    string Message,
    string? ActionUrl,
    string? ActionText,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt,
    DateTimeOffset? ExpiresAt,
    string? Metadata
);

/// <summary>
/// Paged notification list
/// </summary>
public sealed record NotificationListResponse(
    IReadOnlyList<NotificationResponse> Notifications,
    int Total,
    int UnreadCount
);

/// <summary>
/// Create notification request
/// </summary>
public sealed record CreateNotificationRequest(
    string Type,
    string Title,
    string Message,
    string? ActionUrl,
    string? ActionText,
    DateTimeOffset? ExpiresAt,
    string? Metadata
);

/// <summary>
/// Mark notification as read request
/// </summary>
public sealed record MarkAsReadRequest(
    string NotificationId
);

/// <summary>
/// Mark all notifications as read request
/// </summary>
public sealed record MarkAllAsReadRequest;

/// <summary>
/// Delete notification request
/// </summary>
public sealed record DeleteNotificationRequest(
    string NotificationId
);

/// <summary>
/// Notification preferences
/// </summary>
public sealed record NotificationPreferences(
    bool EmailEnabled,
    bool PushEnabled,
    bool InAppEnabled,
    string[] EnabledTypes
);
