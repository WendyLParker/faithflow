using FaithFlow.Backend.Models;

namespace FaithFlow.Api.Tests;

public class RequestServiceTests
{
    private const string UserA = "user-a-sub";
    private const string UserB = "user-b-sub";

    [Fact]
    public async Task AddAsync_PersistsRequestAndAssignsId()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        var request = new Request
        {
            UserId = UserA,
            Title = "Healing for mom",
            Content = "Please restore her strength.",
            RequestTypeId = 2,
        };

        var created = await service.AddAsync(request);

        Assert.NotEqual(0, created.Id);
        Assert.Equal("Healing for mom", created.Title);

        var stored = await context.Requests.FindAsync(created.Id);
        Assert.NotNull(stored);
        Assert.Equal(UserA, stored.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsRequest_WhenUserOwnsIt()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        var created = await service.AddAsync(new Request
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
        var service = TestDbContextFactory.CreateRequestService(context);

        var created = await service.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Private request",
            RequestTypeId = 2,
        });

        var result = await service.GetByIdAsync(created.Id, UserB);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByUserAsync_ReturnsOnlyUsersRequests_OrderedByRequestDateDescending()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        await service.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Older request",
            RequestDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            RequestTypeId = 2,
        });
        await service.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Newer request",
            RequestDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            RequestTypeId = 2,
        });
        await service.AddAsync(new Request
        {
            UserId = UserB,
            Title = "Someone else's request",
            RequestDate = new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc),
            RequestTypeId = 2,
        });

        var results = (await service.GetAllByUserAsync(UserA)).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("Newer request", results[0].Title);
        Assert.Equal("Older request", results[1].Title);
        Assert.All(results, request => Assert.Equal(UserA, request.UserId));
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        var created = await service.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Original title",
            IsCompleted = false,
            RequestTypeId = 2,
        });

        created.Title = "Updated title";
        created.IsCompleted = true;
        created.CompletedDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        await service.UpdateAsync(created);

        var updated = await service.GetByIdAsync(created.Id, UserA);

        Assert.NotNull(updated);
        Assert.Equal("Updated title", updated.Title);
        Assert.True(updated.IsCompleted);
        Assert.Equal(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), updated.CompletedDate);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrueAndRemovesRequest_WhenUserOwnsIt()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        var created = await service.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Temporary request",
            RequestTypeId = 2,
        });

        var deleted = await service.DeleteAsync(created.Id, UserA);

        Assert.True(deleted);
        Assert.Null(await context.Requests.FindAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenRequestDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        var deleted = await service.DeleteAsync(999, UserA);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateRequestService(context);

        var created = await service.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Not yours to delete",
            RequestTypeId = 2,
        });

        var deleted = await service.DeleteAsync(created.Id, UserB);

        Assert.False(deleted);
        Assert.NotNull(await context.Requests.FindAsync(created.Id));
    }
}
