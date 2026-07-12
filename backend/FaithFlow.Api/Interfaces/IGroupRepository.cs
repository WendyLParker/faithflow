using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IGroupRepository
{
    Task<IReadOnlyList<Group>> GetAllAsync();
    Task<Group?> GetByIdAsync(int id);
    Task<Group> CreateAsync(Group group);
    Task<bool> DeleteAsync(int id);
    Task<IReadOnlyList<UserGroup>> GetMembersAsync(int groupId);
    Task<UserGroup?> GetMembershipAsync(int groupId, string userId);
    Task<UserGroup?> GetMembershipByIdAsync(int membershipId);
    Task<IReadOnlyList<UserGroup>> GetMembershipsForUserAsync(string userId);
    Task<UserGroup> AddMemberAsync(int groupId, UserGroup member);
    Task UpdateMemberAsync(UserGroup member);
    Task RemoveMemberAsync(int membershipId);

    /// <summary>Returns all groups with their members loaded.</summary>
    Task<IReadOnlyList<Group>> GetAllWithMembersAsync();

    /// <summary>Returns the requested groups with their members loaded.</summary>
    Task<IReadOnlyList<Group>> GetByIdsWithMembersAsync(IEnumerable<int> groupIds);

    /// <summary>Returns groups the given user has explicit manage permission on.</summary>
    Task<IReadOnlyList<Group>> GetManagedGroupsAsync(string userId);

    /// <summary>Whether the given user has explicit manage permission on the given group.</summary>
    Task<bool> CanManageGroupAsync(string userId, int groupId);

    /// <summary>Returns one membership record per user for enriching profile data.</summary>
    Task<IReadOnlyDictionary<string, UserGroup>> GetPrimaryMembershipByUserAsync();

    /// <summary>
    /// Sets which groups the given user manages: ensures membership (creating it if needed) with
    /// CanManage = true for every group in <paramref name="groupIds"/>, and CanManage = false for
    /// any other group the user is currently a member of. Existing memberships are never removed.
    /// </summary>
    Task SetManagerAssignmentsAsync(string userId, string displayName, string userEmail, IEnumerable<int> groupIds);
}
