using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<IReadOnlyList<UserDepartment>> GetMembersAsync(int departmentId);
    Task<UserDepartment?> GetMembershipAsync(int departmentId, string userId);
    Task<IReadOnlyList<UserDepartment>> GetMembershipsForUserAsync(string userId);
    Task<UserDepartment> AddMemberAsync(int departmentId, UserDepartment member);
    Task UpdateMemberAsync(UserDepartment member);
    Task RemoveMemberAsync(int membershipId);

    /// <summary>Returns all departments that handle the given request type.</summary>
    Task<IReadOnlyList<Department>> GetByRequestTypeAsync(int requestTypeId);
}
