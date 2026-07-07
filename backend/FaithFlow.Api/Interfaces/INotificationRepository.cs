using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetForUserAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification?> GetByIdAsync(int id);
    Task<IReadOnlyList<Notification>> CreateManyAsync(IEnumerable<Notification> notifications);
    Task<Notification> CreateAsync(Notification notification);
    Task MarkReadAsync(int id);
}
