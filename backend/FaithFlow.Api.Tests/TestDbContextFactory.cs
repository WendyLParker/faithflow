using FaithFlow.Backend.Data;
using FaithFlow.Backend.Models;
using FaithFlow.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Api.Tests;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static PrayerService CreatePrayerService(ApplicationDbContext context) => new(context);
}
