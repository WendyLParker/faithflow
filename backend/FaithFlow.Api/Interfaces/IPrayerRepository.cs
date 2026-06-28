using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IPrayerRepository
{
    Task<Prayer> AddAsync(Prayer prayer);
    Task<Prayer?> GetByIdAsync(int id, string userId);
    Task<IEnumerable<Prayer>> GetAllByUserAsync(string userId);
    Task UpdateAsync(Prayer prayer);
    Task<bool> DeleteAsync(int id, string userId);
}