using FaithFlow.Backend.Data;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Services;

public class NotificationService : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(string userId)
    {
        return await _context.Notifications
            .Include(n => n.Prayer)
                .ThenInclude(p => p.RequestType)
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications
            .Include(n => n.Prayer)
                .ThenInclude(p => p.RequestType)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IReadOnlyList<Notification>> CreateManyAsync(IEnumerable<Notification> notifications)
    {
        var list = notifications.ToList();
        _context.Notifications.AddRange(list);
        await _context.SaveChangesAsync();
        return list;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkReadAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}
