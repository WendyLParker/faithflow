using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.Common;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notifications;
    private readonly IRequestRepository _requests;
    private readonly IRequestCommentRepository _comments;
    private readonly IEmailService _email;

    public NotificationController(
        INotificationRepository notifications,
        IRequestRepository requests,
        IRequestCommentRepository comments,
        IEmailService email)
    {
        _notifications = notifications;
        _requests = requests;
        _comments = comments;
        _email = email;
    }

    private string GetCurrentUserId() => ClaimsHelper.GetUserId(User);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notifications.GetForUserAsync(userId);
        var dtos = new List<NotificationDto>();
        foreach (var notification in notifications)
            dtos.Add(await ToDtoAsync(notification));
        return Ok(dtos);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notifications.GetUnreadCountAsync(userId);
        return Ok(new UnreadCountDto { Count = count });
    }

    [HttpPost("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var userId = GetCurrentUserId();
        var notification = await _notifications.GetByIdAsync(id);

        if (notification == null) return NotFound();
        if (notification.RecipientUserId != userId) return Forbid();

        await _notifications.MarkReadAsync(id);

        var request = await _requests.GetByIdAsync(notification.RequestId, notification.Request.UserId);
        if (request == null)
        {
            request = notification.Request;
        }

        if (request.RequestStatus == RequestStatus.New)
        {
            request.RequestStatus = RequestStatus.Acknowledged;
            await _requests.UpdateAsync(request);

            if (request.UserId != userId)
            {
                await _notifications.CreateAsync(new Notification
                {
                    RecipientUserId = request.UserId,
                    RequestId = request.Id,
                    Type = NotificationType.RequestAcknowledged,
                });

                if (!string.IsNullOrWhiteSpace(request.RequesterEmail))
                {
                    var subject = $"Your {request.RequestType?.Name ?? "request"} request has been acknowledged";
                    var body = $@"
                        <p>Hi,</p>
                        <p>Your request <strong>{request.Title}</strong> has been acknowledged by a group member.</p>
                        <p>Thank you for using the Request Tracker portal.</p>";

                    await _email.SendAsync(request.RequesterEmail, subject, body);
                }
            }
        }

        return NoContent();
    }

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

    private async Task<NotificationDto> ToDtoAsync(Notification n)
    {
        string? commentAuthorName = null;
        if (n.Comment != null)
            commentAuthorName = await _comments.ResolveDisplayNameAsync(n.Comment.UserId);

        return new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            RequestId = n.RequestId,
            RequestTitle = n.Request?.Title ?? string.Empty,
            RequestContent = n.Request?.Content,
            RequestTypeName = n.Request?.RequestType?.Name ?? string.Empty,
            RequestStatus = n.Request?.RequestStatus.ToString() ?? string.Empty,
            CommentContent = n.Comment?.Content,
            CommentAuthorName = commentAuthorName,
        };
    }
}
