using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Modules.Identity.Contracts.Notifications;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly AuthDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AuthDbContext db,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<NotificationResponse> CreateNotificationAsync(string userId, CreateNotificationRequest request, CancellationToken ct = default)
    {
        var notification = new UserNotification
        {
            UserId = userId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            ActionUrl = request.ActionUrl,
            ActionText = request.ActionText,
            ExpiresAt = request.ExpiresAt,
            Metadata = request.Metadata
        };

        _db.UserNotifications.Add(notification);
        await _db.SaveChangesAsync(ct);

        var response = MapToResponse(notification);

        // TODO: Send real-time notification when SignalR is properly configured
        // await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationReceived", response);

        _logger.LogInformation("Created notification {NotificationId} for user {UserId}", notification.Id, userId);

        return response;
    }

    public async Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.UserNotifications.AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var total = await query.CountAsync(ct);
        var unreadCount = await _db.UserNotifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationResponse(
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.ActionUrl,
                n.ActionText,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt,
                n.ExpiresAt,
                n.Metadata
            ))
            .ToListAsync(ct);

        return new NotificationListResponse(notifications, total, unreadCount);
    }

    public async Task<bool> MarkAsReadAsync(string userId, string notificationId, CancellationToken ct = default)
    {
        var notification = await _db.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null || notification.IsRead)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO: Send real-time update when SignalR is properly configured
        // await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationMarkedAsRead", notificationId);
        await UpdateUnreadCountAsync(userId);

        _logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var notifications = await _db.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (!notifications.Any())
            return false;

        var now = DateTimeOffset.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _db.SaveChangesAsync(ct);

        // TODO: Send real-time update when SignalR is properly configured
        // await _hubContext.Clients.Group($"user_{userId}").SendAsync("AllNotificationsMarkedAsRead");
        await UpdateUnreadCountAsync(userId);

        _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(string userId, string notificationId, CancellationToken ct = default)
    {
        var notification = await _db.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null)
            return false;

        _db.UserNotifications.Remove(notification);
        await _db.SaveChangesAsync(ct);

        // TODO: Send real-time update when SignalR is properly configured
        // await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationDeleted", notificationId);
        await UpdateUnreadCountAsync(userId);

        _logger.LogInformation("Deleted notification {NotificationId} for user {UserId}", notificationId, userId);

        return true;
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _db.UserNotifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task CleanupExpiredNotificationsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredNotifications = await _db.UserNotifications
            .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt.Value < now)
            .ToListAsync(ct);

        if (expiredNotifications.Any())
        {
            _db.UserNotifications.RemoveRange(expiredNotifications);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Cleaned up {Count} expired notifications", expiredNotifications.Count);
        }
    }

    private async Task UpdateUnreadCountAsync(string userId)
    {
        var unreadCount = await GetUnreadCountAsync(userId);
        // TODO: Send real-time update when SignalR is properly configured
        // await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", unreadCount);
    }

    private static NotificationResponse MapToResponse(UserNotification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.ActionUrl,
            notification.ActionText,
            notification.IsRead,
            notification.CreatedAt,
            notification.ReadAt,
            notification.ExpiresAt,
            notification.Metadata
        );
    }
}
