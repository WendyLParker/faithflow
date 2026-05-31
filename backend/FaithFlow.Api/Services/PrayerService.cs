using FaithFlow.Backend.Models;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;

namespace FaithFlow.Backend.Services
{
    public class PrayerService : IPrayerRepository
    {
        // For now we'll use in-memory storage (SQLite/EF Core coming soon)
        private readonly List<Prayer> _prayers = new();

        public Task<Prayer?> GetByIdAsync(int id, string userId)
        {
            var prayer = _prayers.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            return Task.FromResult(prayer);
        }

        public Task<IEnumerable<Prayer>> GetAllByUserAsync(string userId)
        {
            var userPrayers = _prayers.Where(p => p.UserId == userId);
            return Task.FromResult(userPrayers);
        }

        public Task<Prayer> AddAsync(Prayer prayer)
        {
            prayer.Id = _prayers.Count + 1;
            _prayers.Add(prayer);
            return Task.FromResult(prayer);
        }

        public Task UpdateAsync(Prayer prayer)
        {
            var existing = _prayers.FirstOrDefault(p => p.Id == prayer.Id);
            if (existing != null)
            {
                existing.Title = prayer.Title;
                existing.Content = prayer.Content;
                existing.IsAnswered = prayer.IsAnswered;
                existing.AnsweredDate = prayer.AnsweredDate;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id, string userId)
        {
            var prayer = _prayers.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (prayer != null)
                _prayers.Remove(prayer);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(int id, string userId)
        {
            var exists = _prayers.Any(p => p.Id == id && p.UserId == userId);
            return Task.FromResult(exists);
        }
    }
}