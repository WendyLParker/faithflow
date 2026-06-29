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

    public PrayerController(IPrayerRepository prayerService)
    {
        _prayerService = prayerService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value
            ?? User.FindFirst("cognito:username")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? "unknown-user";
    }

    // GET: api/prayer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrayerResponseDto>>> GetAll()
    {
        var userId = GetCurrentUserId();
        var prayers = await _prayerService.GetAllByUserAsync(userId);

        var dtos = prayers.Select(p => new PrayerResponseDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            PrayerDate = p.PrayerDate,
            IsAnswered = p.IsAnswered,
            AnsweredDate = p.AnsweredDate,
            Categories = p.Categories,
            VoiceNoteUrl = p.VoiceNoteUrl,
            ImageUrl = p.ImageUrl,
            StreakDays = p.StreakDays
        });

        return Ok(dtos);
    }

    // GET: api/prayer/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PrayerResponseDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var prayer = await _prayerService.GetByIdAsync(id, userId);

        if (prayer == null) return NotFound();

        var dto = new PrayerResponseDto
        {
            Id = prayer.Id,
            Title = prayer.Title,
            Content = prayer.Content,
            PrayerDate = prayer.PrayerDate,
            IsAnswered = prayer.IsAnswered,
            AnsweredDate = prayer.AnsweredDate,
            Categories = prayer.Categories,
            VoiceNoteUrl = prayer.VoiceNoteUrl,
            ImageUrl = prayer.ImageUrl,
            StreakDays = prayer.StreakDays
        };

        return Ok(dto);
    }

    // POST: api/prayer
    [HttpPost]
    public async Task<ActionResult<PrayerResponseDto>> Create([FromBody] PrayerCreateDto dto)
    {
        var userId = GetCurrentUserId();

        var prayer = new Prayer
        {
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            Categories = dto.Categories ?? new List<string>(),
            PrayerDate = DateTime.UtcNow
        };

        var created = await _prayerService.AddAsync(prayer);

        var response = new PrayerResponseDto
        {
            Id = created.Id,
            Title = created.Title,
            Content = created.Content,
            PrayerDate = created.PrayerDate,
            Categories = created.Categories,
            IsAnswered = created.IsAnswered
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
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

        var response = new PrayerResponseDto
        {
            Id = prayer.Id,
            Title = prayer.Title,
            Content = prayer.Content,
            PrayerDate = prayer.PrayerDate,
            IsAnswered = prayer.IsAnswered,
            AnsweredDate = prayer.AnsweredDate,
            Categories = prayer.Categories
        };

        return Ok(response);
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
}