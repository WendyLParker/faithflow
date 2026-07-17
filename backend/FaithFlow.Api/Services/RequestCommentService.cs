using FaithFlow.Backend.Data;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Services;

public class RequestCommentService : IRequestCommentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IRequestRepository _requests;

    public RequestCommentService(ApplicationDbContext context, IRequestRepository requests)
    {
        _context = context;
        _requests = requests;
    }

    public async Task<IReadOnlyList<RequestComment>?> GetByRequestAsync(int requestId, string userId)
    {
        if (await _requests.GetByIdAsync(requestId, userId) == null)
            return null;

        return await _context.RequestComments
            .Where(c => c.RequestId == requestId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<RequestComment?> AddAsync(int requestId, string userId, string content)
    {
        var request = await _requests.GetByIdAsync(requestId, userId);
        if (request == null) return null;

        var comment = new RequestComment
        {
            RequestId = requestId,
            UserId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        _context.RequestComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<IReadOnlyList<string>> GetPriorAssigneeCommenterUserIdsAsync(
        int requestId,
        string requestOwnerUserId,
        int excludeCommentId)
    {
        return await _context.RequestComments
            .AsNoTracking()
            .Where(c => c.RequestId == requestId
                && c.Id != excludeCommentId
                && c.UserId != requestOwnerUserId)
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<string> ResolveDisplayNameAsync(string userId)
    {
        var role = await _context.UserRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(ur => ur.UserId == userId);

        if (!string.IsNullOrWhiteSpace(role?.DisplayName))
            return role.DisplayName;

        var groupMember = await _context.UserGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(ug => ug.UserId == userId);

        if (!string.IsNullOrWhiteSpace(groupMember?.DisplayName))
            return groupMember.DisplayName;

        return "Team member";
    }
}
