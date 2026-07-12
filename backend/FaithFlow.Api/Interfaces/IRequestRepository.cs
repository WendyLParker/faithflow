using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IRequestRepository
{
    Task<Request> AddAsync(Request request);
    Task<Request?> GetByIdAsync(int id, string userId);
    Task<IEnumerable<Request>> GetAllByUserAsync(string userId);
    Task UpdateAsync(Request request);
    Task<bool> DeleteAsync(int id, string userId);
    Task SetAssignedGroupsAsync(int requestId, IEnumerable<int> groupIds);
}
