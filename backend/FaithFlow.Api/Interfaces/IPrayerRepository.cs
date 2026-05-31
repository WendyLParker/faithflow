using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces
{
    public interface IPrayerRepository
    {
        Task<Prayer?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<Prayer>> GetAllByUserAsync(string userId);
        Task<Prayer> AddAsync(Prayer prayer);
        Task UpdateAsync(Prayer prayer);
        Task DeleteAsync(int id, string userId);
        Task<bool> ExistsAsync(int id, string userId);
    }
}