using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly IGroupRepository _groups;
    private readonly IUserRoleRepository _roles;

    public GroupController(IGroupRepository groups, IUserRoleRepository roles)
    {
        _groups = groups;
        _roles = roles;
    }

    private string GetCurrentUserId() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst("cognito:username")?.Value
        ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
        ?? "unknown-user";

    // GET /api/group
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetAll()
    {
        var groups = await _groups.GetAllAsync();
        return Ok(groups.Select(ToGroupDto));
    }

    // GET /api/group/managed — groups the current user can manage (all groups if global admin)
    [HttpGet("managed")]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetManaged()
    {
        var userId = GetCurrentUserId();

        if (await _roles.IsAdminAsync(userId))
        {
            var all = await _groups.GetAllAsync();
            return Ok(all.Select(ToGroupDto));
        }

        var managed = await _groups.GetManagedGroupsAsync(userId);
        return Ok(managed.Select(ToGroupDto));
    }

    // POST /api/group  (admin only)
    [HttpPost]
    public async Task<ActionResult<GroupDto>> Create([FromBody] CreateGroupDto dto)
    {
        var userId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(userId)) return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Group name is required.");

        var group = new Group
        {
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        };

        var created = await _groups.CreateAsync(group);
        return CreatedAtAction(nameof(GetAll), ToGroupDto(created));
    }

    // DELETE /api/group/{id}  (admin only)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(userId)) return Forbid();

        var deleted = await _groups.DeleteAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // GET /api/group/manager-assignments/{userId}  (admin only)
    [HttpGet("manager-assignments/{userId}")]
    public async Task<ActionResult<IEnumerable<GroupManagerAssignmentDto>>> GetManagerAssignments(string userId)
    {
        var currentUserId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(currentUserId)) return Forbid();

        var allGroups = await _groups.GetAllAsync();
        var memberships = await _groups.GetMembershipsForUserAsync(userId);

        var result = allGroups.Select(g => new GroupManagerAssignmentDto
        {
            GroupId = g.Id,
            GroupName = g.Name,
            CanManage = memberships.FirstOrDefault(m => m.GroupId == g.Id)?.CanManage ?? false,
        });

        return Ok(result);
    }

    // POST /api/group/manager-assignments  (admin only)
    [HttpPost("manager-assignments")]
    public async Task<IActionResult> SetManagerAssignments([FromBody] SetGroupManagersDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(currentUserId)) return Forbid();

        if (string.IsNullOrWhiteSpace(dto.UserId))
            return BadRequest("A user must be selected.");

        await _groups.SetManagerAssignmentsAsync(dto.UserId, dto.DisplayName, dto.UserEmail, dto.GroupIds);
        return NoContent();
    }

    // GET /api/group/{id}/members
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetMembers(int id)
    {
        var group = await _groups.GetByIdAsync(id);
        if (group == null) return NotFound();

        var members = await _groups.GetMembersAsync(id);
        return Ok(members.Select(ToMemberDto));
    }

    // GET /api/group/my
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetMine()
    {
        var userId = GetCurrentUserId();
        var memberships = await _groups.GetMembershipsForUserAsync(userId);
        return Ok(memberships.Select(ToMemberDto));
    }

    // POST /api/group/{id}/members
    [HttpPost("{id}/members")]
    public async Task<ActionResult<GroupMemberDto>> AddMember(int id, [FromBody] AddGroupMemberDto dto)
    {
        var group = await _groups.GetByIdAsync(id);
        if (group == null) return NotFound("Group not found.");

        var userId = GetCurrentUserId();
        var isAddingSelf = dto.UserId == userId;
        if (!isAddingSelf)
        {
            var canManage = await _roles.IsAdminAsync(userId) || await _groups.CanManageGroupAsync(userId, id);
            if (!canManage) return Forbid();
        }

        var existing = await _groups.GetMembershipAsync(id, dto.UserId);
        if (existing != null)
            return Conflict("This user is already a member of this group.");

        var member = new UserGroup
        {
            UserId = dto.UserId,
            DisplayName = dto.DisplayName,
            UserEmail = dto.UserEmail,
            GroupId = id,
            EmailNotificationsEnabled = dto.EmailNotificationsEnabled,
        };

        var created = await _groups.AddMemberAsync(id, member);
        return CreatedAtAction(nameof(GetMembers), new { id }, ToMemberDto(created));
    }

    // PATCH /api/group/members/{membershipId}/email-preference
    [HttpPatch("members/{membershipId}/email-preference")]
    public async Task<IActionResult> UpdateEmailPreference(int membershipId, [FromBody] UpdateEmailPreferenceDto dto)
    {
        var userId = GetCurrentUserId();
        var memberships = await _groups.GetMembershipsForUserAsync(userId);
        var record = memberships.FirstOrDefault(m => m.Id == membershipId);

        if (record == null) return NotFound();

        record.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
        await _groups.UpdateMemberAsync(record);
        return NoContent();
    }

    // PATCH /api/group/members/{membershipId}/manager  (admin only)
    [HttpPatch("members/{membershipId}/manager")]
    public async Task<IActionResult> UpdateManager(int membershipId, [FromBody] UpdateManagerDto dto)
    {
        var userId = GetCurrentUserId();
        if (!await _roles.IsAdminAsync(userId)) return Forbid();

        var record = await _groups.GetMembershipByIdAsync(membershipId);
        if (record == null) return NotFound();

        record.CanManage = dto.CanManage;
        await _groups.UpdateMemberAsync(record);
        return NoContent();
    }

    // DELETE /api/group/members/{membershipId}
    [HttpDelete("members/{membershipId}")]
    public async Task<IActionResult> RemoveMember(int membershipId)
    {
        var userId = GetCurrentUserId();
        var record = await _groups.GetMembershipByIdAsync(membershipId);
        if (record == null) return NoContent();

        var isSelf = record.UserId == userId;
        if (!isSelf)
        {
            var canManage = await _roles.IsAdminAsync(userId) || await _groups.CanManageGroupAsync(userId, record.GroupId);
            if (!canManage) return Forbid();
        }

        await _groups.RemoveMemberAsync(membershipId);
        return NoContent();
    }

    private static GroupDto ToGroupDto(Group g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Description = g.Description,
    };

    private static GroupMemberDto ToMemberDto(UserGroup ug) => new()
    {
        Id = ug.Id,
        UserId = ug.UserId,
        DisplayName = ug.DisplayName,
        UserEmail = ug.UserEmail,
        EmailNotificationsEnabled = ug.EmailNotificationsEnabled,
        GroupId = ug.GroupId,
        GroupName = ug.Group?.Name ?? string.Empty,
        CanManage = ug.CanManage,
    };
}
