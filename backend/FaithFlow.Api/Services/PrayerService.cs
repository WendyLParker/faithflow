using FaithFlow.Backend.Models;
using FaithFlow.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using FaithFlow.Backend.Data;

namespace FaithFlow.Backend.Services;

public class PrayerService : IPrayerRepository
{
    private readonly ApplicationDbContext _context;

    public PrayerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Prayer> AddAsync(Prayer prayer)
    {
        _context.Prayers.Add(prayer);
        await _context.SaveChangesAsync();
        return prayer;
    }

    public async Task<Prayer?> GetByIdAsync(int id, string userId)
    {
        return await _context.Prayers
            .Include(p => p.ProgressNotes)
            .Include(p => p.RequestType)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<IEnumerable<Prayer>> GetAllByUserAsync(string userId)
    {
        return await _context.Prayers
            .Include(p => p.ProgressNotes)
            .Include(p => p.RequestType)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PrayerDate)
            .ToListAsync();
    }

    public async Task UpdateAsync(Prayer prayer)
    {
        _context.Prayers.Update(prayer);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var prayer = await _context.Prayers.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (prayer == null) return false;

        _context.Prayers.Remove(prayer);
        await _context.SaveChangesAsync();
        return true;
    }
}