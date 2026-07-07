using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notifications;
    private readonly IPrayerRepository _prayers;
    private readonly IDepartmentRepository _departments;
    private readonly IEmailService _email;

    public NotificationController(
        INotificationRepository notifications,
        IPrayerRepository prayers,
        IDepartmentRepository departments,
        IEmailService email)
    {
        _notifications = notifications;
        _prayers = prayers;
        _departments = departments;
        _email = email;
    }

    private string GetCurrentUserId() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst("cognito:username")?.Value
        ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
        ?? "unknown-user";

    // GET /api/notification
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notifications.GetForUserAsync(userId);
        return Ok(notifications.Select(ToDto));
    }

    // GET /api/notification/unread-count
    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notifications.GetUnreadCountAsync(userId);
        return Ok(new UnreadCountDto { Count = count });
    }

    // POST /api/notification/{id}/acknowledge
    [HttpPost("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var userId = GetCurrentUserId();
        var notification = await _notifications.GetByIdAsync(id);

        if (notification == null) return NotFound();
        if (notification.RecipientUserId != userId) return Forbid();

        // Mark this notification as read
        await _notifications.MarkReadAsync(id);

        // Load the prayer
        var prayer = await _prayers.GetByIdAsync(notification.PrayerId, notification.Prayer.UserId);
        if (prayer == null)
        {
            // Fallback: try loading without user scope
            prayer = notification.Prayer;
        }

        // Advance status to Acknowledged if still New
        if (prayer.RequestStatus == RequestStatus.New)
        {
            prayer.RequestStatus = RequestStatus.Acknowledged;
            await _prayers.UpdateAsync(prayer);

            // Notify the requester that their request was acknowledged
            if (prayer.UserId != userId)
            {
                await _notifications.CreateAsync(new Notification
                {
                    RecipientUserId = prayer.UserId,
                    PrayerId = prayer.Id,
                    Type = NotificationType.RequestAcknowledged,
                });

                // Email the requester if they have an email on file
                if (!string.IsNullOrWhiteSpace(prayer.RequesterEmail))
                {
                    var subject = $"Your {prayer.RequestType?.Name ?? "request"} request has been acknowledged";
                    var body = $@"
                        <p>Hi,</p>
                        <p>Your request <strong>{prayer.Title}</strong> has been acknowledged by a department member.</p>
                        <p>Thank you for using the Request Tracker portal.</p>";

                    await _email.SendAsync(prayer.RequesterEmail, subject, body);
                }
            }
        }

        return NoContent();
    }

    // POST /api/notification/{id}/dismiss  (for RequestAcknowledged notifications — just marks read)
    [HttpPost("{id}/dismiss")]
    public async Task<IActionResult> Dismiss(int id)
    {
        var userId = GetCurrentUserId();
        var notification = await _notifications.GetByIdAsync(id);

        if (notification == null) return NotFound();
        if (notification.RecipientUserId != userId) return Forbid();

        await _notifications.MarkReadAsync(id);
        return NoContent();
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type.ToString(),
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        PrayerId = n.PrayerId,
        PrayerTitle = n.Prayer?.Title ?? string.Empty,
        PrayerContent = n.Prayer?.Content,
        RequestTypeName = n.Prayer?.RequestType?.Name ?? string.Empty,
        RequestStatus = n.Prayer?.RequestStatus.ToString() ?? string.Empty,
    };
}
