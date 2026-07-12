using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProgressNoteController : ControllerBase
{
    private readonly IProgressNoteRepository _progressNoteService;

    public ProgressNoteController(IProgressNoteRepository progressNoteService)
    {
        _progressNoteService = progressNoteService;
    }

    private string GetCurrentUserId() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst("cognito:username")?.Value
        ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
        ?? "unknown-user";

    [HttpGet("request/{requestId}")]
    public async Task<ActionResult<IEnumerable<ProgressNoteResponseDto>>> GetByRequest(int requestId)
    {
        var userId = GetCurrentUserId();
        var notes = await _progressNoteService.GetByRequestAsync(requestId, userId);

        var dtos = notes.Select(n => new ProgressNoteResponseDto
        {
            Id = n.Id,
            RequestId = n.RequestId,
            Content = n.Content,
            EntryDate = n.EntryDate,
            MoodRating = n.MoodRating
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<ProgressNoteResponseDto>> Create([FromBody] ProgressNoteCreateDto dto)
    {
        var userId = GetCurrentUserId();

        var note = new ProgressNote
        {
            RequestId = dto.RequestId,
            UserId = userId,
            Content = dto.Content,
            MoodRating = dto.MoodRating
        };

        var created = await _progressNoteService.AddAsync(note);

        var response = new ProgressNoteResponseDto
        {
            Id = created.Id,
            RequestId = created.RequestId,
            Content = created.Content,
            EntryDate = created.EntryDate,
            MoodRating = created.MoodRating
        };

        return CreatedAtAction(nameof(GetByRequest), new { requestId = created.RequestId }, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _progressNoteService.DeleteAsync(id, userId);

        if (!success) return NotFound();

        return NoContent();
    }
}
