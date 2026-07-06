using FaithFlow.Backend.Data;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Services;

public class RequestTypeService : IRequestTypeRepository
{
    private readonly ApplicationDbContext _context;

    public RequestTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RequestType>> GetAllAsync()
    {
        return await _context.RequestTypes
            .OrderBy(rt => rt.Id)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.RequestTypes.AnyAsync(rt => rt.Id == id);
    }
}
