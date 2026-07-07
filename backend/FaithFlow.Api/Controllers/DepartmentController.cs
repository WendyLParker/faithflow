using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _departments;

    public DepartmentController(IDepartmentRepository departments)
    {
        _departments = departments;
    }

    private string GetCurrentUserId() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst("cognito:username")?.Value
        ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
        ?? "unknown-user";

    // GET /api/department
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
    {
        var departments = await _departments.GetAllAsync();
        return Ok(departments.Select(ToDepartmentDto));
    }

    // GET /api/department/{id}/members
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<DepartmentMemberDto>>> GetMembers(int id)
    {
        var dept = await _departments.GetByIdAsync(id);
        if (dept == null) return NotFound();

        var members = await _departments.GetMembersAsync(id);
        return Ok(members.Select(ToMemberDto));
    }

    // GET /api/department/my
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<DepartmentMemberDto>>> GetMine()
    {
        var userId = GetCurrentUserId();
        var memberships = await _departments.GetMembershipsForUserAsync(userId);
        return Ok(memberships.Select(ToMemberDto));
    }

    // POST /api/department/{id}/members
    [HttpPost("{id}/members")]
    public async Task<ActionResult<DepartmentMemberDto>> AddMember(int id, [FromBody] AddDepartmentMemberDto dto)
    {
        var dept = await _departments.GetByIdAsync(id);
        if (dept == null) return NotFound("Department not found.");

        var existing = await _departments.GetMembershipAsync(id, dto.UserId);
        if (existing != null)
            return Conflict("This user is already a member of this department.");

        var member = new UserDepartment
        {
            UserId = dto.UserId,
            DisplayName = dto.DisplayName,
            UserEmail = dto.UserEmail,
            DepartmentId = id,
            EmailNotificationsEnabled = dto.EmailNotificationsEnabled,
        };

        var created = await _departments.AddMemberAsync(id, member);
        return CreatedAtAction(nameof(GetMembers), new { id }, ToMemberDto(created));
    }

    // PATCH /api/department/members/{membershipId}/email-preference
    [HttpPatch("members/{membershipId}/email-preference")]
    public async Task<IActionResult> UpdateEmailPreference(int membershipId, [FromBody] UpdateEmailPreferenceDto dto)
    {
        var userId = GetCurrentUserId();
        var memberships = await _departments.GetMembershipsForUserAsync(userId);
        var record = memberships.FirstOrDefault(m => m.Id == membershipId);

        if (record == null) return NotFound();

        record.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
        await _departments.UpdateMemberAsync(record);
        return NoContent();
    }

    // DELETE /api/department/members/{membershipId}
    [HttpDelete("members/{membershipId}")]
    public async Task<IActionResult> RemoveMember(int membershipId)
    {
        await _departments.RemoveMemberAsync(membershipId);
        return NoContent();
    }

    private static DepartmentDto ToDepartmentDto(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        RequestTypeNames = d.DepartmentRequestTypes
            .Select(drt => drt.RequestType?.Name ?? string.Empty)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList(),
    };

    private static DepartmentMemberDto ToMemberDto(UserDepartment ud) => new()
    {
        Id = ud.Id,
        UserId = ud.UserId,
        DisplayName = ud.DisplayName,
        UserEmail = ud.UserEmail,
        EmailNotificationsEnabled = ud.EmailNotificationsEnabled,
        DepartmentId = ud.DepartmentId,
        DepartmentName = ud.Department?.Name ?? string.Empty,
    };
}
