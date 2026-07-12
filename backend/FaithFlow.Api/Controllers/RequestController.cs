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
    private readonly INotificationRepository _notifications;
    private readonly IGroupRepository _groups;
    private readonly IUserRoleRepository _roles;
    private readonly IEmailService _email;

    public RequestController(
        IRequestRepository requests,
        INotificationRepository notifications,
        IGroupRepository groups,
        IUserRoleRepository roles,
        IEmailService email)
    {
        _requests = requests;
        _notifications = notifications;
        _groups = groups;
        _roles = roles;
        _email = email;
    }

    private string GetCurrentUserId() => ClaimsHelper.GetUserId(User);

    private string? GetCurrentUserEmail() => ClaimsHelper.GetEmail(User);

    private static RequestResponseDto ToDto(Request request) => new()
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
        VoiceNoteUrl = request.VoiceNoteUrl,
        ImageUrl = request.ImageUrl,
        StreakDays = request.StreakDays,
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetAll()
    {
        var userId = GetCurrentUserId();
        var requests = await _requests.GetAllByUserAsync(userId);
        return Ok(requests.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RequestResponseDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var request = await _requests.GetByIdAsync(id, userId);
        if (request == null) return NotFound();
        return Ok(ToDto(request));
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

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(loaded!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RequestResponseDto>> Update(int id, [FromBody] RequestUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var request = await _requests.GetByIdAsync(id, userId);
        if (request == null) return NotFound();

        if (dto.Content != null) request.Content = dto.Content;
        if (dto.IsCompleted.HasValue)
        {
            request.IsCompleted = dto.IsCompleted.Value;
            request.CompletedDate = dto.IsCompleted.Value ? DateTime.UtcNow : null;
        }

        await _requests.UpdateAsync(request);
        var updated = await _requests.GetByIdAsync(id, userId);
        return Ok(ToDto(updated!));
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
