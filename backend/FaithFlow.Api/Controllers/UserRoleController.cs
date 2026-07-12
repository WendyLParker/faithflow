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
public class UserRoleController : ControllerBase
{
    private readonly IUserRoleRepository _roles;
    private readonly IGroupRepository _groups;

    public UserRoleController(IUserRoleRepository roles, IGroupRepository groups)
    {
        _roles = roles;
        _groups = groups;
    }

    private string GetCurrentUserId() => ClaimsHelper.GetUserId(User);

    private string? GetCurrentUserEmail() => ClaimsHelper.GetEmail(User);

    // GET /api/userrole/me
    [HttpGet("me")]
    public async Task<ActionResult<MyRoleDto>> GetMine([FromQuery] string? email)
    {
        var userId = GetCurrentUserId();
        var resolvedEmail = GetCurrentUserEmail() ?? email;
        var displayName = ClaimsHelper.DefaultDisplayNameFromEmail(resolvedEmail);
        var record = await _roles.GetOrCreateMyRoleAsync(userId, displayName, resolvedEmail);

        return Ok(new MyRoleDto
        {
            UserId = record.UserId,
            Role = record.Role.ToString(),
            IsAdmin = record.Role == AppRole.Admin,
            DisplayName = record.DisplayName,
            ProfileColor = record.ProfileColor,
        });
    }

    // PATCH /api/userrole/me
    [HttpPatch("me")]
    public async Task<ActionResult<MyRoleDto>> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        var record = await _roles.UpdateProfileAsync(userId, dto.DisplayName, dto.ProfileColor);

        return Ok(new MyRoleDto
        {
            UserId = record.UserId,
            Role = record.Role.ToString(),
            IsAdmin = record.Role == AppRole.Admin,
            DisplayName = record.DisplayName,
            ProfileColor = record.ProfileColor,
        });
    }

    // GET /api/userrole  (admin only)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetAll()
    {
        var userId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(userId)) return Forbid();

        var all = await _roles.GetAllAsync();
        var membershipsByUser = await _groups.GetPrimaryMembershipByUserAsync();

        return Ok(all.Select(r => ToDtoEnriched(r, membershipsByUser)));
    }

    // POST /api/userrole  (admin only) — assign/change a user's role
    [HttpPost]
    public async Task<ActionResult<UserRoleDto>> SetRole([FromBody] SetUserRoleDto dto)
    {
        var userId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(userId)) return Forbid();

        if (!Enum.TryParse<AppRole>(dto.Role, ignoreCase: true, out var role))
            return BadRequest("Invalid role. Must be 'Admin' or 'Member'.");

        var updated = await _roles.SetRoleAsync(dto.UserId, dto.DisplayName, dto.UserEmail, role);
        return Ok(ToDto(updated));
    }

    private static UserRoleDto ToDto(UserRole r) => new()
    {
        UserId = r.UserId,
        DisplayName = r.DisplayName,
        UserEmail = r.UserEmail,
        Role = r.Role.ToString(),
    };

    private static UserRoleDto ToDtoEnriched(
        UserRole r,
        IReadOnlyDictionary<string, Models.UserGroup> membershipsByUser) => new()
    {
        UserId = r.UserId,
        DisplayName = !string.IsNullOrWhiteSpace(r.DisplayName)
            ? r.DisplayName
            : membershipsByUser.GetValueOrDefault(r.UserId)?.DisplayName ?? string.Empty,
        UserEmail = !string.IsNullOrWhiteSpace(r.UserEmail)
            ? r.UserEmail
            : membershipsByUser.GetValueOrDefault(r.UserId)?.UserEmail ?? string.Empty,
        Role = r.Role.ToString(),
    };
}
