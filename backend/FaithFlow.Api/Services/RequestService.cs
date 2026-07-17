using FaithFlow.Backend.Models;
using FaithFlow.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using FaithFlow.Backend.Data;

namespace FaithFlow.Backend.Services;

public class RequestService : IRequestRepository
{
    private readonly ApplicationDbContext _context;

    public RequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Request> AddAsync(Request request)
    {
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<Request?> GetByIdAsync(int id, string userId)
    {
        var request = await _context.Requests
            .Include(r => r.ProgressNotes)
            .Include(r => r.RequestType)
            .Include(r => r.AssignedGroups)
                .ThenInclude(rg => rg.Group)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return null;
        if (request.UserId == userId) return request;
        if (await IsRecipientAsync(request, userId)) return request;
        return null;
    }

    public async Task<IEnumerable<Request>> GetAllByUserAsync(string userId)
    {
        return await _context.Requests
            .Include(r => r.ProgressNotes)
            .Include(r => r.RequestType)
            .Include(r => r.AssignedGroups)
                .ThenInclude(rg => rg.Group)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetReceivedByUserAsync(string userId)
    {
        var groupIds = await _context.UserGroups
            .Where(ug => ug.UserId == userId)
            .Select(ug => ug.GroupId)
            .ToListAsync();

        var isAdmin = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role == AppRole.Admin);

        return await _context.Requests
            .Include(r => r.ProgressNotes)
            .Include(r => r.RequestType)
            .Include(r => r.AssignedGroups)
                .ThenInclude(rg => rg.Group)
            .Where(r => r.UserId != userId && (
                r.AssignedGroups.Any(rg => groupIds.Contains(rg.GroupId)) ||
                (isAdmin && !r.AssignedGroups.Any())))
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task UpdateAsync(Request request)
    {
        _context.Requests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (request == null) return false;

        _context.Requests.Remove(request);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetAssignedGroupsAsync(int requestId, IEnumerable<int> groupIds)
    {
        var ids = groupIds.Distinct().ToList();
        var existing = await _context.RequestGroups
            .Where(rg => rg.RequestId == requestId)
            .ToListAsync();

        _context.RequestGroups.RemoveRange(existing);

        foreach (var groupId in ids)
        {
            _context.RequestGroups.Add(new RequestGroup
            {
                RequestId = requestId,
                GroupId = groupId,
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsRecipientAsync(Request request, string userId)
    {
        var groupIds = await _context.UserGroups
            .Where(ug => ug.UserId == userId)
            .Select(ug => ug.GroupId)
            .ToListAsync();

        if (request.AssignedGroups.Any(rg => groupIds.Contains(rg.GroupId)))
            return true;

        var isAdmin = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role == AppRole.Admin);

        return isAdmin && !request.AssignedGroups.Any();
    }

    public async Task<Request?> MarkFulfilledAsync(int id, string userId)
    {
        var request = await _context.Requests
            .Include(r => r.RequestType)
            .Include(r => r.AssignedGroups)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return null;
        if (request.UserId == userId) return null;
        if (request.IsCompleted) return null;
        if (request.RequestStatus == RequestStatus.Fulfilled) return null;
        if (!await IsRecipientAsync(request, userId)) return null;

        request.RequestStatus = RequestStatus.Fulfilled;
        request.FulfilledDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<Request?> CloseAsync(int id, string userId)
    {
        var request = await _context.Requests
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (request == null) return null;
        if (request.IsCompleted) return null;
        if (request.RequestStatus != RequestStatus.Fulfilled) return null;

        request.IsCompleted = true;
        request.CompletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return request;
    }
}
