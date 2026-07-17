using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IRequestRepository
{
    Task<Request> AddAsync(Request request);
    Task<Request?> GetByIdAsync(int id, string userId);
    Task<IEnumerable<Request>> GetAllByUserAsync(string userId);
    Task<IEnumerable<Request>> GetReceivedByUserAsync(string userId);
    Task UpdateAsync(Request request);
    Task<bool> DeleteAsync(int id, string userId);
    Task SetAssignedGroupsAsync(int requestId, IEnumerable<int> groupIds);
    Task<bool> IsRecipientAsync(Request request, string userId);
    Task<Request?> MarkFulfilledAsync(int id, string userId);
    Task<Request?> CloseAsync(int id, string userId);
}
