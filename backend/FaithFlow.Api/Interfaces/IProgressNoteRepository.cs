using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Interfaces;

public interface IProgressNoteRepository
{
    Task<ProgressNote> AddAsync(ProgressNote note);
    Task<IEnumerable<ProgressNote>> GetByPrayerAsync(int prayerId, string userId);
    Task<bool> DeleteAsync(int id, string userId);
}