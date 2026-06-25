using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace FaithFlow.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrayerController : ControllerBase
    {
        private readonly IPrayerRepository _prayerService;
        private readonly IConfiguration _configuration;

        public PrayerController(IPrayerRepository prayerService, IConfiguration configuration)
        {
            _prayerService = prayerService;
            _configuration = configuration;
        }

        private string GetCurrentUserId()
        {
            // 1. Try standard claims first
            var sub = User.FindFirst("sub")?.Value
                   ?? User.FindFirst("cognito:username")?.Value
                   ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(sub))
                return sub;

            // 2. Manual fallback - parse token and attach claims
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                try
                {
                    var token = authHeader.Substring(7).Trim();
                    var parts = token.Split('.');
                    if (parts.Length > 1)
                    {
                        var payload = parts[1];
                        payload = payload.PadRight((payload.Length + 3) & ~3, '=');

                        var bytes = Convert.FromBase64String(payload);
                        var json = System.Text.Encoding.UTF8.GetString(bytes);

                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        var userId = root.TryGetProperty("sub", out var subProp)
                            ? subProp.GetString()
                            : "unknown-user";

                        // === Build and attach claims ===
                        var claims = new List<Claim>();

                        foreach (var prop in root.EnumerateObject())
                        {
                            claims.Add(new Claim(prop.Name, prop.Value.ToString() ?? ""));
                        }

                        // Add standard NameIdentifier claim
                        claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId));

                        var identity = new ClaimsIdentity(claims, "Cognito");
                        HttpContext.User = new ClaimsPrincipal(identity);

                        Console.WriteLine($"✅ Manual claims attached for user: {userId}");
                        return userId;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Manual token parsing failed: {ex.Message}");
                }
            }

            return "unknown-user";
        }

        [HttpGet("debug-token")]
        [AllowAnonymous]
        public IActionResult DebugToken()
        {
            var token = Request.Headers.Authorization.FirstOrDefault();
            var userId = GetCurrentUserId();   // your existing method

            return Ok(new
            {
                hasToken = !string.IsNullOrEmpty(token),
                tokenPreview = token?.Substring(0, 50) + "...",
                userId = userId,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        [HttpGet("debug-auth")]
        [AllowAnonymous]
        public IActionResult DebugAuth()
        {
            var authority = _configuration["Cognito:Authority"];   // You'll need to inject IConfiguration
            var clientId = _configuration["Cognito:ClientId"];

            var authHeader = Request.Headers.Authorization.FirstOrDefault();

            Console.WriteLine("=== AUTH DEBUG ===");
            Console.WriteLine($"Authority: {authority}");
            Console.WriteLine($"ClientId : {clientId}");
            Console.WriteLine($"Auth Header: {authHeader}");
            Console.WriteLine($"Token length: {authHeader?.Length ?? 0}");
            Console.WriteLine("==================");

            return Ok(new
            {
                authority,
                clientId,
                hasAuthHeader = !string.IsNullOrEmpty(authHeader),
                tokenPreview = authHeader?.Substring(0, Math.Min(100, authHeader.Length)) + "...",
                tokenLength = authHeader?.Length ?? 0
            });
        }

        [HttpGet("debug-raw-token")]
        [AllowAnonymous]
        public IActionResult DebugRawToken()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest("No Bearer token found");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Simple base64 decode of the payload (middle part)
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                    return BadRequest("Invalid token format");

                var payload = parts[1];
                // Add padding if needed
                payload = payload.PadRight((payload.Length + 3) & ~3, '=');

                var bytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(bytes);

                return Ok(new
                {
                    rawTokenLength = token.Length,
                    payload = json
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to decode: {ex.Message}");
            }
        }
        // GET: api/prayer
        [HttpGet]
        [Authorize]
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
        [Authorize]
        public async Task<ActionResult<PrayerResponseDto>> GetById(int id)
        {
            var userId = GetCurrentUserId();
            var prayer = await _prayerService.GetByIdAsync(id, userId);

            if (prayer == null)
                return NotFound();

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
        [Authorize]
        public async Task<ActionResult<PrayerResponseDto>> Create([FromBody] PrayerCreateDto dto)
        {
            var userId = GetCurrentUserId();

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
                Categories = created.Categories,
                IsAnswered = created.IsAnswered
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        // PUT: api/prayer/5   (Update prayer)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<PrayerResponseDto>> Update(int id, [FromBody] PrayerUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            var prayer = await _prayerService.GetByIdAsync(id, userId);

            if (prayer == null) return NotFound();

            // Update fields
            if (dto.Content != null) prayer.Content = dto.Content;
            if (dto.IsAnswered.HasValue)
            {
                prayer.IsAnswered = dto.IsAnswered.Value;
                prayer.AnsweredDate = dto.IsAnswered.Value ? DateTime.UtcNow : null;
            }

            await _prayerService.UpdateAsync(prayer);

            // Return updated prayer
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
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var prayer = await _prayerService.GetByIdAsync(id, userId);

            if (prayer == null) return NotFound();

            await _prayerService.DeleteAsync(id, userId);
            return NoContent();   // 204 No Content = success
        }

    }
}