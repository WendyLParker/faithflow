using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;

namespace FaithFlow.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrayerController : ControllerBase
    {
        private readonly IPrayerRepository _prayerService;

        public PrayerController(IPrayerRepository prayerService)
        {
            _prayerService = prayerService;
        }

        // GET: api/prayer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrayerResponseDto>>> GetAll()
        {
            var userId = User.Identity?.Name; // Will come from Cognito JWT later
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var prayer = await _prayerService.GetByIdAsync(id, userId);
            if (prayer == null) return NotFound();

            // Convert to DTO...
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
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var prayer = new Prayer
            {
                UserId = userId,
                Title = dto.Title,
                Content = dto.Content,
                Categories = dto.Categories,
                PrayerDate = DateTime.UtcNow
            };

            var created = await _prayerService.AddAsync(prayer);

            var response = new PrayerResponseDto
            {
                Id = created.Id,
                Title = created.Title,
                Content = created.Content,
                PrayerDate = created.PrayerDate,
                Categories = created.Categories
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
    }
}