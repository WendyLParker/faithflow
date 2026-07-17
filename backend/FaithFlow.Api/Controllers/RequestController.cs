using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FaithFlow.Backend.Common;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RequestController : ControllerBase
{
    private readonly IRequestRepository _requests;
    private readonly IRequestCommentRepository _comments;
    private readonly INotificationRepository _notifications;
    private readonly IGroupRepository _groups;
    private readonly IUserRoleRepository _roles;
    private readonly IEmailService _email;

    public RequestController(
        IRequestRepository requests,
        IRequestCommentRepository comments,
        INotificationRepository notifications,
        IGroupRepository groups,
        IUserRoleRepository roles,
        IEmailService email)
    {
        _requests = requests;
        _comments = comments;
        _notifications = notifications;
        _groups = groups;
        _roles = roles;
        _email = email;
    }

    private string GetCurrentUserId() => ClaimsHelper.GetUserId(User);

    private string? GetCurrentUserEmail() => ClaimsHelper.GetEmail(User);

    private async Task<RequestResponseDto> ToDtoAsync(Request request, string currentUserId)
    {
        var isOwner = request.UserId == currentUserId;
        var isRecipient = !isOwner && await _requests.IsRecipientAsync(request, currentUserId);

        return new RequestResponseDto
        {
            Id = request.Id,
            Title = request.Title,
            Content = request.Content,
            RequestDate = request.RequestDate,
            IsCompleted = request.IsCompleted,
            CompletedDate = request.CompletedDate,
            RequestTypeId = request.RequestTypeId,
            RequestTypeName = request.RequestType?.Name ?? string.Empty,
            GroupNames = request.AssignedGroups
                .Select(rg => rg.Group?.Name ?? string.Empty)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList(),
            RequestStatus = request.RequestStatus.ToString(),
            FulfilledDate = request.FulfilledDate,
            IsOwnedByCurrentUser = isOwner,
            CanFulfill = isRecipient
                && !request.IsCompleted
                && request.RequestStatus != RequestStatus.Fulfilled,
            CanClose = isOwner
                && !request.IsCompleted
                && request.RequestStatus == RequestStatus.Fulfilled,
            VoiceNoteUrl = request.VoiceNoteUrl,
            ImageUrl = request.ImageUrl,
            StreakDays = request.StreakDays,
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetAll([FromQuery] string scope = "sent")
    {
        var userId = GetCurrentUserId();
        var requests = scope.Equals("received", StringComparison.OrdinalIgnoreCase)
            ? await _requests.GetReceivedByUserAsync(userId)
            : await _requests.GetAllByUserAsync(userId);
        return Ok(await Task.WhenAll(requests.Select(r => ToDtoAsync(r, userId))));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RequestResponseDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var request = await _requests.GetByIdAsync(id, userId);
        if (request == null) return NotFound();
        return Ok(await ToDtoAsync(request, userId));
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> Create([FromBody] RequestCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var requesterEmail = GetCurrentUserEmail();

        var request = new Request
        {
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            RequestTypeId = dto.RequestTypeId,
            RequesterEmail = requesterEmail,
            RequestDate = DateTime.UtcNow,
            RequestStatus = RequestStatus.New,
        };

        var created = await _requests.AddAsync(request);
        var groupIds = dto.GroupIds?.Distinct().ToList() ?? new List<int>();

        if (groupIds.Count > 0)
            await _requests.SetAssignedGroupsAsync(created.Id, groupIds);

        var loaded = await _requests.GetByIdAsync(created.Id, userId);

        await FanOutNewRequestNotificationsAsync(loaded!, groupIds);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, await ToDtoAsync(loaded!, userId));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RequestResponseDto>> Update(int id, [FromBody] RequestUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var request = await _requests.GetByIdAsync(id, userId);
        if (request == null) return NotFound();

        if (request.UserId != userId) return Forbid();

        if (dto.Content != null) request.Content = dto.Content;
        if (dto.IsCompleted.HasValue)
        {
            return BadRequest("Use POST /api/request/{id}/close to close a fulfilled request.");
        }

        await _requests.UpdateAsync(request);
        var updated = await _requests.GetByIdAsync(id, userId);
        return Ok(await ToDtoAsync(updated!, userId));
    }

    [HttpPost("{id}/fulfill")]
    public async Task<ActionResult<RequestResponseDto>> Fulfill(int id)
    {
        var userId = GetCurrentUserId();
        var request = await _requests.MarkFulfilledAsync(id, userId);
        if (request == null) return NotFound();

        await _notifications.CreateAsync(new Notification
        {
            RecipientUserId = request.UserId,
            RequestId = request.Id,
            Type = NotificationType.RequestFulfilled,
        });

        if (!string.IsNullOrWhiteSpace(request.RequesterEmail))
        {
            var subject = $"Your {request.RequestType?.Name ?? "request"} request has been completed";
            var body = $@"
                <p>Hi,</p>
                <p>Your request <strong>{request.Title}</strong> has been marked complete by a group member.</p>
                <p>Log in to the Request Tracker to review and close the request.</p>";

            await _email.SendAsync(request.RequesterEmail, subject, body);
        }

        var loaded = await _requests.GetByIdAsync(id, userId);
        return Ok(await ToDtoAsync(loaded!, userId));
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<RequestResponseDto>> Close(int id)
    {
        var userId = GetCurrentUserId();
        var request = await _requests.CloseAsync(id, userId);
        if (request == null) return NotFound();

        var loaded = await _requests.GetByIdAsync(id, userId);
        return Ok(await ToDtoAsync(loaded!, userId));
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<RequestCommentResponseDto>>> GetComments(int id)
    {
        var userId = GetCurrentUserId();
        var comments = await _comments.GetByRequestAsync(id, userId);
        if (comments == null) return NotFound();

        var dtos = new List<RequestCommentResponseDto>();
        foreach (var comment in comments)
        {
            dtos.Add(await ToCommentDtoAsync(comment, userId));
        }

        return Ok(dtos);
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<RequestCommentResponseDto>> AddComment(int id, [FromBody] RequestCommentCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Comment content is required.");

        if (dto.Content.Length > 2000)
            return BadRequest("Comment must be 2000 characters or fewer.");

        var userId = GetCurrentUserId();
        var comment = await _comments.AddAsync(id, userId, dto.Content);
        if (comment == null) return NotFound();

        var request = await _requests.GetByIdAsync(id, userId);
        if (request != null)
        {
            var authorName = await _comments.ResolveDisplayNameAsync(userId);

            if (request.UserId != userId)
            {
                await NotifyRequestorOfCommentAsync(request, comment, authorName);
            }
            else
            {
                await NotifyAssigneesOfCommentAsync(request, comment, authorName);
            }
        }

        return Ok(await ToCommentDtoAsync(comment, userId));
    }

    private async Task NotifyRequestorOfCommentAsync(Request request, RequestComment comment, string authorName)
    {
        await _notifications.CreateAsync(new Notification
        {
            RecipientUserId = request.UserId,
            RequestId = request.Id,
            CommentId = comment.Id,
            Type = NotificationType.RequestComment,
        });

        if (!string.IsNullOrWhiteSpace(request.RequesterEmail))
        {
            var subject = $"New comment on your {request.RequestType?.Name ?? "request"} request";
            var body = $@"
                <p>Hi,</p>
                <p><strong>{authorName}</strong> left a comment on your request <strong>{request.Title}</strong>:</p>
                <p>{comment.Content}</p>
                <p>Log in to the Request Tracker to view and reply.</p>";

            await _email.SendAsync(request.RequesterEmail, subject, body);
        }
    }

    private async Task NotifyAssigneesOfCommentAsync(Request request, RequestComment comment, string authorName)
    {
        var recipientIds = await _comments.GetPriorAssigneeCommenterUserIdsAsync(
            request.Id, request.UserId, comment.Id);

        if (recipientIds.Count == 0) return;

        var notifications = recipientIds
            .Select(recipientId => new Notification
            {
                RecipientUserId = recipientId,
                RequestId = request.Id,
                CommentId = comment.Id,
                Type = NotificationType.RequestComment,
            })
            .ToList();

        await _notifications.CreateManyAsync(notifications);

        foreach (var recipientId in recipientIds)
        {
            var email = await ResolveUserEmailAsync(recipientId);
            if (string.IsNullOrWhiteSpace(email)) continue;

            var subject = $"Reply on {request.RequestType?.Name ?? "request"} request: {request.Title}";
            var body = $@"
                <p>Hi,</p>
                <p><strong>{authorName}</strong> replied on request <strong>{request.Title}</strong>:</p>
                <p>{comment.Content}</p>
                <p>Log in to the Request Tracker to view and respond.</p>";

            await _email.SendAsync(email, subject, body);
        }
    }

    private async Task<string?> ResolveUserEmailAsync(string userId)
    {
        var role = await _roles.GetByUserIdAsync(userId);
        if (!string.IsNullOrWhiteSpace(role?.UserEmail))
            return role.UserEmail;

        var memberships = await _groups.GetMembershipsForUserAsync(userId);
        return memberships.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.UserEmail))?.UserEmail;
    }

    private async Task<RequestCommentResponseDto> ToCommentDtoAsync(RequestComment comment, string currentUserId)
    {
        return new RequestCommentResponseDto
        {
            Id = comment.Id,
            RequestId = comment.RequestId,
            AuthorName = await _comments.ResolveDisplayNameAsync(comment.UserId),
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            IsOwnComment = comment.UserId == currentUserId,
        };
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _requests.DeleteAsync(id, userId);
        if (!success) return NotFound();
        return NoContent();
    }

    private async Task FanOutNewRequestNotificationsAsync(Request request, IReadOnlyList<int> groupIds)
    {
        var recipientUserIds = new HashSet<string>();
        var emailTargets = new List<(string Email, string GroupName)>();

        if (groupIds.Count > 0)
        {
            var groups = await _groups.GetByIdsWithMembersAsync(groupIds);
            foreach (var group in groups)
            {
                foreach (var member in group.Members)
                {
                    recipientUserIds.Add(member.UserId);
                    if (member.EmailNotificationsEnabled && !string.IsNullOrWhiteSpace(member.UserEmail))
                        emailTargets.Add((member.UserEmail, group.Name));
                }
            }
        }
        else
        {
            var admins = await _roles.GetAdminsAsync();
            foreach (var admin in admins)
            {
                recipientUserIds.Add(admin.UserId);
                if (!string.IsNullOrWhiteSpace(admin.UserEmail))
                    emailTargets.Add((admin.UserEmail, "Admin"));
            }
        }

        if (recipientUserIds.Count == 0) return;

        var notifications = recipientUserIds
            .Select(recipientId => new Notification
            {
                RecipientUserId = recipientId,
                RequestId = request.Id,
                Type = NotificationType.NewRequest,
            })
            .ToList();

        await _notifications.CreateManyAsync(notifications);

        foreach (var (email, groupName) in emailTargets.Distinct())
        {
            var subject = $"[{groupName}] New {request.RequestType?.Name ?? "request"}: {request.Title}";
            var body = $@"
                <p>A new request has been submitted.</p>
                <p><strong>{request.Title}</strong></p>
                {(string.IsNullOrWhiteSpace(request.Content) ? "" : $"<p>{request.Content}</p>")}
                <p>Log in to the Request Tracker to acknowledge or review this request.</p>";

            await _email.SendAsync(email, subject, body);
        }
    }
}
