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

        var context = new ApplicationDbContext(options);
        SeedRequestTypes(context);
        return context;
    }

    private static void SeedRequestTypes(ApplicationDbContext context)
    {
        if (context.RequestTypes.Any())
        {
            return;
        }

        context.RequestTypes.AddRange(
            new RequestType { Id = 1, Name = "Ride" },
            new RequestType { Id = 2, Name = "Prayer" },
            new RequestType { Id = 3, Name = "Supply" },
            new RequestType { Id = 4, Name = "Service" },
            new RequestType { Id = 5, Name = "Labor" }
        );
        context.SaveChanges();
    }

    public static PrayerService CreatePrayerService(ApplicationDbContext context) => new(context);
}
