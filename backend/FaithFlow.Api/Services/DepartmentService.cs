using FaithFlow.Backend.Data;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Services;

public class DepartmentService : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.DepartmentRequestTypes)
                .ThenInclude(drt => drt.RequestType)
            .OrderBy(d => d.Id)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.DepartmentRequestTypes)
                .ThenInclude(drt => drt.RequestType)
            .Include(d => d.Members)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IReadOnlyList<UserDepartment>> GetMembersAsync(int departmentId)
    {
        return await _context.UserDepartments
            .Include(ud => ud.Department)
            .Where(ud => ud.DepartmentId == departmentId)
            .OrderBy(ud => ud.DisplayName)
            .ToListAsync();
    }

    public async Task<UserDepartment?> GetMembershipAsync(int departmentId, string userId)
    {
        return await _context.UserDepartments
            .FirstOrDefaultAsync(ud => ud.DepartmentId == departmentId && ud.UserId == userId);
    }

    public async Task<IReadOnlyList<UserDepartment>> GetMembershipsForUserAsync(string userId)
    {
        return await _context.UserDepartments
            .Include(ud => ud.Department)
            .Where(ud => ud.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserDepartment> AddMemberAsync(int departmentId, UserDepartment member)
    {
        member.DepartmentId = departmentId;
        _context.UserDepartments.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task UpdateMemberAsync(UserDepartment member)
    {
        _context.UserDepartments.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int membershipId)
    {
        var record = await _context.UserDepartments.FindAsync(membershipId);
        if (record != null)
        {
            _context.UserDepartments.Remove(record);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IReadOnlyList<Department>> GetByRequestTypeAsync(int requestTypeId)
    {
        return await _context.Departments
            .Include(d => d.Members)
            .Where(d => d.DepartmentRequestTypes.Any(drt => drt.RequestTypeId == requestTypeId))
            .ToListAsync();
    }
}
