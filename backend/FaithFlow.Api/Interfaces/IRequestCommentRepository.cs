using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IRequestCommentRepository
{
    Task<IReadOnlyList<RequestComment>?> GetByRequestAsync(int requestId, string userId);
    Task<RequestComment?> AddAsync(int requestId, string userId, string content);
    Task<IReadOnlyList<string>> GetPriorAssigneeCommenterUserIdsAsync(int requestId, string requestOwnerUserId, int excludeCommentId);
    Task<string> ResolveDisplayNameAsync(string userId);
}
