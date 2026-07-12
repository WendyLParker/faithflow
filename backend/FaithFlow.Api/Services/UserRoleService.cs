using FaithFlow.Backend.Data;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.Common;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Services;

public class UserRoleService : IUserRoleRepository
{
    private readonly ApplicationDbContext _context;

    public UserRoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserRole> GetOrCreateMyRoleAsync(string userId, string? displayName, string? email)
    {
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        var resolvedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? ClaimsHelper.DefaultDisplayNameFromEmail(normalizedEmail)
            : displayName.Trim();

        var existing = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userId);
        if (existing != null)
        {
            var updated = false;

            if (string.IsNullOrWhiteSpace(existing.UserEmail) && !string.IsNullOrWhiteSpace(normalizedEmail))
            {
                existing.UserEmail = normalizedEmail;
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(existing.DisplayName))
            {
                var name = resolvedDisplayName
                    ?? ClaimsHelper.DefaultDisplayNameFromEmail(existing.UserEmail)
                    ?? ClaimsHelper.DefaultDisplayNameFromEmail(normalizedEmail);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    existing.DisplayName = name;
                    updated = true;
                }
            }

            if (updated)
                await _context.SaveChangesAsync();

            return existing;
        }

        // The first person to ever check their role becomes the initial Admin,
        // so there's always at least one admin able to manage roles and groups.
        var anyRolesExist = await _context.UserRoles.AnyAsync();

        var record = new UserRole
        {
            UserId = userId,
            DisplayName = resolvedDisplayName
                ?? ClaimsHelper.DefaultDisplayNameFromEmail(normalizedEmail)
                ?? string.Empty,
            UserEmail = normalizedEmail ?? string.Empty,
            Role = anyRolesExist ? AppRole.Member : AppRole.Admin,
        };

        _context.UserRoles.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<IReadOnlyList<UserRole>> GetAllAsync()
    {
        return await _context.UserRoles
            .OrderByDescending(r => r.Role)
            .ThenBy(r => r.DisplayName)
            .ToListAsync();
    }

    public async Task<UserRole?> GetByUserIdAsync(string userId)
    {
        return await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task<UserRole> SetRoleAsync(string userId, string displayName, string userEmail, AppRole role)
    {
        var existing = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userId);
        if (existing != null)
        {
            existing.Role = role;
            if (!string.IsNullOrWhiteSpace(displayName)) existing.DisplayName = displayName;
            if (!string.IsNullOrWhiteSpace(userEmail)) existing.UserEmail = userEmail;
            await _context.SaveChangesAsync();
            return existing;
        }

        var record = new UserRole
        {
            UserId = userId,
            DisplayName = displayName,
            UserEmail = userEmail,
            Role = role,
        };
        _context.UserRoles.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<bool> IsAdminAsync(string userId)
    {
        var record = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userId);
        return record?.Role == AppRole.Admin;
    }

    public async Task<IReadOnlyList<UserRole>> GetAdminsAsync()
    {
        return await _context.UserRoles
            .Where(r => r.Role == AppRole.Admin)
            .ToListAsync();
    }

    public async Task<UserRole> UpdateProfileAsync(string userId, string? displayName, string? profileColor)
    {
        var record = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userId);
        if (record == null)
            throw new InvalidOperationException("User role record not found.");

        if (displayName != null)
            record.DisplayName = displayName.Trim();
        if (profileColor != null)
            record.ProfileColor = profileColor.Trim();

        await _context.SaveChangesAsync();
        return record;
    }
}
