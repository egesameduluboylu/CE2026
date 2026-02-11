namespace Modules.Identity.Contracts.Notifications;

public interface INotificationService
{
    Task<NotificationResponse> CreateNotificationAsync(string userId, CreateNotificationRequest request, CancellationToken ct = default);
    Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(string userId, string notificationId, CancellationToken ct = default);
    Task<bool> MarkAllAsReadAsync(string userId, CancellationToken ct = default);
    Task<bool> DeleteNotificationAsync(string userId, string notificationId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    Task CleanupExpiredNotificationsAsync(CancellationToken ct = default);
}
