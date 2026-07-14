using FaithFlow.Backend.Data;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Services;

public class GroupService : IGroupRepository
{
    private readonly ApplicationDbContext _context;

    public GroupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Group>> GetAllAsync()
    {
        return await _context.Groups
            .OrderBy(g => g.Id)
            .ToListAsync();
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Group> CreateAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null) return false;

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<UserGroup>> GetMembersAsync(int groupId)
    {
        return await _context.UserGroups
            .Include(ug => ug.Group)
            .Where(ug => ug.GroupId == groupId)
            .OrderBy(ug => ug.DisplayName)
            .ToListAsync();
    }

    public async Task<UserGroup?> GetMembershipAsync(int groupId, string userId)
    {
        return await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId);
    }

    public async Task<UserGroup?> GetMembershipByIdAsync(int membershipId)
    {
        return await _context.UserGroups
            .Include(ug => ug.Group)
            .FirstOrDefaultAsync(ug => ug.Id == membershipId);
    }

    public async Task<IReadOnlyList<UserGroup>> GetMembershipsForUserAsync(string userId)
    {
        return await _context.UserGroups
            .Include(ug => ug.Group)
            .Where(ug => ug.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserGroup> AddMemberAsync(int groupId, UserGroup member)
    {
        member.GroupId = groupId;
        _context.UserGroups.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task UpdateMemberAsync(UserGroup member)
    {
        _context.UserGroups.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int membershipId)
    {
        var record = await _context.UserGroups.FindAsync(membershipId);
        if (record != null)
        {
            _context.UserGroups.Remove(record);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IReadOnlyList<Group>> GetAllWithMembersAsync()
    {
        return await _context.Groups
            .Include(g => g.Members)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Group>> GetByIdsWithMembersAsync(IEnumerable<int> groupIds)
    {
        var ids = groupIds.Distinct().ToList();
        if (ids.Count == 0) return Array.Empty<Group>();

        return await _context.Groups
            .Include(g => g.Members)
            .Where(g => ids.Contains(g.Id))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Group>> GetManagedGroupsAsync(string userId)
    {
        return await _context.Groups
            .Where(g => g.Members.Any(m => m.UserId == userId && m.CanManage == true))
            .OrderBy(g => g.Id)
            .ToListAsync();
    }

    public async Task<bool> CanManageGroupAsync(string userId, int groupId)
    {
        return await _context.UserGroups
            .AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId && ug.CanManage == true);
    }

    public async Task<IReadOnlyDictionary<string, UserGroup>> GetPrimaryMembershipByUserAsync()
    {
        var memberships = await _context.UserGroups.AsNoTracking().ToListAsync();
        return memberships
            .GroupBy(m => m.UserId)
            .ToDictionary(g => g.Key, g => g.First());
    }

    public async Task SetManagerAssignmentsAsync(string userId, string displayName, string userEmail, IEnumerable<int> groupIds)
    {
        var targetIds = groupIds.Distinct().ToHashSet();
        var existingMemberships = await _context.UserGroups
            .Where(ug => ug.UserId == userId)
            .ToListAsync();

        foreach (var membership in existingMemberships)
        {
            membership.CanManage = targetIds.Contains(membership.GroupId);
        }

        var existingGroupIds = existingMemberships.Select(m => m.GroupId).ToHashSet();
        foreach (var groupId in targetIds.Except(existingGroupIds))
        {
            _context.UserGroups.Add(new UserGroup
            {
                UserId = userId,
                DisplayName = displayName,
                UserEmail = userEmail,
                GroupId = groupId,
                EmailNotificationsEnabled = true,
                CanManage = true,
            });
        }

        await _context.SaveChangesAsync();
    }
}
