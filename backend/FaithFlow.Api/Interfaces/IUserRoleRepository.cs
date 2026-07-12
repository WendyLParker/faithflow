using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IUserRoleRepository
{
    /// <summary>
    /// Returns the caller's role record, creating one if it doesn't exist yet.
    /// The very first user to ever call this (i.e. no roles exist in the system)
    /// is automatically bootstrapped as an Admin.
    /// </summary>
    Task<UserRole> GetOrCreateMyRoleAsync(string userId, string? displayName, string? email);

    Task<IReadOnlyList<UserRole>> GetAllAsync();

    Task<UserRole?> GetByUserIdAsync(string userId);

    Task<UserRole> SetRoleAsync(string userId, string displayName, string userEmail, AppRole role);

    Task<bool> IsAdminAsync(string userId);

    Task<IReadOnlyList<UserRole>> GetAdminsAsync();

    Task<UserRole> UpdateProfileAsync(string userId, string? displayName, string? profileColor);
}
