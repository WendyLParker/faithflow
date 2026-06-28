using FaithFlow.Backend.Models;
using FaithFlow.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using FaithFlow.Backend.Data;

namespace FaithFlow.Backend.Services;

public class ProgressNoteService : IProgressNoteRepository
{
    private readonly ApplicationDbContext _context;

    public ProgressNoteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProgressNote> AddAsync(ProgressNote note)
    {
        _context.ProgressNotes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<IEnumerable<ProgressNote>> GetByPrayerAsync(int prayerId, string userId)
    {
        return await _context.ProgressNotes
            .Where(pn => pn.PrayerId == prayerId && pn.UserId == userId)
            .OrderByDescending(pn => pn.EntryDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var note = await _context.ProgressNotes.FirstOrDefaultAsync(pn => pn.Id == id && pn.UserId == userId);
        if (note == null) return false;

        _context.ProgressNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }
}