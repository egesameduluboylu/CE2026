using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Contracts.Notifications;
using System.Security.Claims;
using BuildingBlocks.Web;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? User.FindFirst("sub")?.Value 
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, unreadOnly, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching notifications.", HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId, ct);
            return Ok(ApiResponse.Ok(new { count }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching unread count.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.CreateNotificationAsync(userId, request, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating notification.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("{notificationId}/mark-read")]
    public async Task<IActionResult> MarkAsRead(string notificationId, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.MarkAsReadAsync(userId, notificationId, ct);
            return Ok(ApiResponse.Ok(new { success = result }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while marking notification as read.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.MarkAllAsReadAsync(userId, ct);
            return Ok(ApiResponse.Ok(new { success = result }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while marking all notifications as read.", HttpContext.TraceIdentifier));
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(string notificationId, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.DeleteNotificationAsync(userId, notificationId, ct);
            return Ok(ApiResponse.Ok(new { success = result }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while deleting notification.", HttpContext.TraceIdentifier));
        }
    }
}
