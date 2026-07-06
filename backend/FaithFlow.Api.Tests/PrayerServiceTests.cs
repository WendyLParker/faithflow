using FaithFlow.Backend.Models;

namespace FaithFlow.Api.Tests;

public class PrayerServiceTests
{
    private const string UserA = "user-a-sub";
    private const string UserB = "user-b-sub";

    [Fact]
    public async Task AddAsync_PersistsPrayerAndAssignsId()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var prayer = new Prayer
        {
            UserId = UserA,
            Title = "Healing for mom",
            Content = "Please restore her strength.",
            Categories = new List<string> { "Health", "Family" },
            RequestTypeId = 2,
        };

        var created = await service.AddAsync(prayer);

        Assert.NotEqual(0, created.Id);
        Assert.Equal("Healing for mom", created.Title);

        var stored = await context.Prayers.FindAsync(created.Id);
        Assert.NotNull(stored);
        Assert.Equal(UserA, stored.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPrayer_WhenUserOwnsIt()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var created = await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Job interview",
            RequestTypeId = 2,
        });

        var result = await service.GetByIdAsync(created.Id, UserA);

        Assert.NotNull(result);
        Assert.Equal("Job interview", result.Title);
        Assert.NotNull(result.ProgressNotes);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnIt()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var created = await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Private request",
            RequestTypeId = 2,
        });

        var result = await service.GetByIdAsync(created.Id, UserB);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByUserAsync_ReturnsOnlyUsersPrayers_OrderedByPrayerDateDescending()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Older request",
            PrayerDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            RequestTypeId = 2,
        });
        await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Newer request",
            PrayerDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            RequestTypeId = 2,
        });
        await service.AddAsync(new Prayer
        {
            UserId = UserB,
            Title = "Someone else's request",
            PrayerDate = new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc),
            RequestTypeId = 2,
        });

        var results = (await service.GetAllByUserAsync(UserA)).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("Newer request", results[0].Title);
        Assert.Equal("Older request", results[1].Title);
        Assert.All(results, prayer => Assert.Equal(UserA, prayer.UserId));
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var created = await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Original title",
            IsAnswered = false,
            RequestTypeId = 2,
        });

        created.Title = "Updated title";
        created.IsAnswered = true;
        created.AnsweredDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        await service.UpdateAsync(created);

        var updated = await service.GetByIdAsync(created.Id, UserA);

        Assert.NotNull(updated);
        Assert.Equal("Updated title", updated.Title);
        Assert.True(updated.IsAnswered);
        Assert.Equal(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), updated.AnsweredDate);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrueAndRemovesPrayer_WhenUserOwnsIt()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var created = await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Temporary request",
            RequestTypeId = 2,
        });

        var deleted = await service.DeleteAsync(created.Id, UserA);

        Assert.True(deleted);
        Assert.Null(await context.Prayers.FindAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenPrayerDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var deleted = await service.DeleteAsync(999, UserA);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnPrayer()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreatePrayerService(context);

        var created = await service.AddAsync(new Prayer
        {
            UserId = UserA,
            Title = "Not yours to delete",
            RequestTypeId = 2,
        });

        var deleted = await service.DeleteAsync(created.Id, UserB);

        Assert.False(deleted);
        Assert.NotNull(await context.Prayers.FindAsync(created.Id));
    }
}
