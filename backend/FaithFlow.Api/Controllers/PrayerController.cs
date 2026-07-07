using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PrayerController : ControllerBase
{
    private readonly IPrayerRepository _prayerService;
    private readonly INotificationRepository _notifications;
    private readonly IDepartmentRepository _departments;
    private readonly IEmailService _email;

    public PrayerController(
        IPrayerRepository prayerService,
        INotificationRepository notifications,
        IDepartmentRepository departments,
        IEmailService email)
    {
        _prayerService = prayerService;
        _notifications = notifications;
        _departments = departments;
        _email = email;
    }

    private string GetCurrentUserId() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst("cognito:username")?.Value
        ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
        ?? "unknown-user";

    private string? GetCurrentUserEmail() =>
        User.FindFirst("email")?.Value;

    private static PrayerResponseDto ToDto(Prayer prayer) => new()
    {
        Id = prayer.Id,
        Title = prayer.Title,
        Content = prayer.Content,
        PrayerDate = prayer.PrayerDate,
        IsAnswered = prayer.IsAnswered,
        AnsweredDate = prayer.AnsweredDate,
        Categories = prayer.Categories,
        RequestTypeId = prayer.RequestTypeId,
        RequestTypeName = prayer.RequestType?.Name ?? string.Empty,
        RequestStatus = prayer.RequestStatus.ToString(),
        VoiceNoteUrl = prayer.VoiceNoteUrl,
        ImageUrl = prayer.ImageUrl,
        StreakDays = prayer.StreakDays,
    };

    // GET: api/prayer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrayerResponseDto>>> GetAll()
    {
        var userId = GetCurrentUserId();
        var prayers = await _prayerService.GetAllByUserAsync(userId);
        return Ok(prayers.Select(ToDto));
    }

    // GET: api/prayer/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PrayerResponseDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var prayer = await _prayerService.GetByIdAsync(id, userId);
        if (prayer == null) return NotFound();
        return Ok(ToDto(prayer));
    }

    // POST: api/prayer
    [HttpPost]
    public async Task<ActionResult<PrayerResponseDto>> Create([FromBody] PrayerCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var requesterEmail = GetCurrentUserEmail();

        var prayer = new Prayer
        {
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            Categories = dto.Categories ?? new List<string>(),
            RequestTypeId = dto.RequestTypeId,
            RequesterEmail = requesterEmail,
            PrayerDate = DateTime.UtcNow,
            RequestStatus = RequestStatus.New,
        };

        var created = await _prayerService.AddAsync(prayer);
        var loaded = await _prayerService.GetByIdAsync(created.Id, userId);

        // Fan-out notifications to all members of the handling department(s)
        await FanOutNewRequestNotificationsAsync(loaded!);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(loaded!));
    }

    // PUT: api/prayer/5
    [HttpPut("{id}")]
    public async Task<ActionResult<PrayerResponseDto>> Update(int id, [FromBody] PrayerUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var prayer = await _prayerService.GetByIdAsync(id, userId);
        if (prayer == null) return NotFound();

        if (dto.Content != null) prayer.Content = dto.Content;
        if (dto.IsAnswered.HasValue)
        {
            prayer.IsAnswered = dto.IsAnswered.Value;
            prayer.AnsweredDate = dto.IsAnswered.Value ? DateTime.UtcNow : null;
        }

        await _prayerService.UpdateAsync(prayer);
        var updated = await _prayerService.GetByIdAsync(id, userId);
        return Ok(ToDto(updated!));
    }

    // DELETE: api/prayer/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _prayerService.DeleteAsync(id, userId);
        if (!success) return NotFound();
        return NoContent();
    }

    // ── Notification fan-out ─────────────────────────────────────────────

    private async Task FanOutNewRequestNotificationsAsync(Prayer prayer)
    {
        var departments = await _departments.GetByRequestTypeAsync(prayer.RequestTypeId);
        var allMembers = departments.SelectMany(d => d.Members).ToList();

        if (!allMembers.Any()) return;

        var notifications = allMembers
            //.Where(m => m.UserId != prayer.UserId)
            .Select(m => new Notification
            {
                RecipientUserId = m.UserId,
                PrayerId = prayer.Id,
                Type = NotificationType.NewRequest,
            })
            .ToList();

        if (notifications.Any())
        {
            await _notifications.CreateManyAsync(notifications);
        }

        // Email members who have opted in
        foreach (var member in allMembers.Where(m => m.EmailNotificationsEnabled && !string.IsNullOrWhiteSpace(m.UserEmail)))
        {
            var dept = departments.FirstOrDefault(d => d.Members.Any(m => m.Id == member.Id));
            var subject = $"[{dept?.Name ?? "Your Department"}] New {prayer.RequestType?.Name ?? "request"}: {prayer.Title}";
            var body = $@"
                <p>A new request has been submitted to your department.</p>
                <p><strong>{prayer.Title}</strong></p>
                {(string.IsNullOrWhiteSpace(prayer.Content) ? "" : $"<p>{prayer.Content}</p>")}
                <p>Log in to the Request Tracker to acknowledge or review this request.</p>";

            await _email.SendAsync(member.UserEmail, subject, body);
        }
    }
}
