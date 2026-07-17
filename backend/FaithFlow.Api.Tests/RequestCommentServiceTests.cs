using FaithFlow.Backend.Models;

namespace FaithFlow.Api.Tests;

public class RequestCommentServiceTests
{
    private const string UserA = "user-a-sub";
    private const string UserB = "user-b-sub";

    [Fact]
    public async Task AddAsync_PersistsComment_WhenUserCanAccessRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var requests = TestDbContextFactory.CreateRequestService(context);
        var comments = TestDbContextFactory.CreateRequestCommentService(context);

        context.Groups.Add(new Group { Id = 10, Name = "Test Group" });
        context.UserGroups.Add(new UserGroup
        {
            UserId = UserB,
            GroupId = 10,
            DisplayName = "Member B",
            UserEmail = "b@test.com",
        });
        await context.SaveChangesAsync();

        var created = await requests.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Needs comment",
            RequestTypeId = 2,
        });
        await requests.SetAssignedGroupsAsync(created.Id, new[] { 10 });

        var comment = await comments.AddAsync(created.Id, UserB, "On my way!");

        Assert.NotNull(comment);
        Assert.Equal("On my way!", comment.Content);
        Assert.Equal(UserB, comment.UserId);
    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenUserCannotAccessRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var requests = TestDbContextFactory.CreateRequestService(context);
        var comments = TestDbContextFactory.CreateRequestCommentService(context);

        var created = await requests.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Private",
            RequestTypeId = 2,
        });

        var comment = await comments.AddAsync(created.Id, UserB, "Should fail");

        Assert.Null(comment);
    }

    [Fact]
    public async Task GetByRequestAsync_ReturnsCommentsInOrder()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var requests = TestDbContextFactory.CreateRequestService(context);
        var comments = TestDbContextFactory.CreateRequestCommentService(context);

        var created = await requests.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Thread",
            RequestTypeId = 2,
        });

        await comments.AddAsync(created.Id, UserA, "First");
        await Task.Delay(5);
        await comments.AddAsync(created.Id, UserA, "Second");

        var result = await comments.GetByRequestAsync(created.Id, UserA);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("First", result[0].Content);
        Assert.Equal("Second", result[1].Content);
    }

    [Fact]
    public async Task GetPriorAssigneeCommenterUserIdsAsync_ReturnsDistinctNonOwnerCommenters()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var requests = TestDbContextFactory.CreateRequestService(context);
        var comments = TestDbContextFactory.CreateRequestCommentService(context);

        context.Groups.Add(new Group { Id = 10, Name = "Test Group" });
        context.UserGroups.AddRange(
            new UserGroup { UserId = UserB, GroupId = 10, DisplayName = "Member B", UserEmail = "b@test.com" },
            new UserGroup { UserId = "user-c-sub", GroupId = 10, DisplayName = "Member C", UserEmail = "c@test.com" });
        await context.SaveChangesAsync();

        var created = await requests.AddAsync(new Request
        {
            UserId = UserA,
            Title = "Thread",
            RequestTypeId = 2,
        });
        await requests.SetAssignedGroupsAsync(created.Id, new[] { 10 });

        var commentB1 = await comments.AddAsync(created.Id, UserB, "Assignee reply 1");
        await comments.AddAsync(created.Id, UserA, "Requestor reply");
        var commentC = await comments.AddAsync(created.Id, "user-c-sub", "Another assignee");
        var requestorReply = await comments.AddAsync(created.Id, UserA, "Requestor again");

        Assert.NotNull(commentB1);
        Assert.NotNull(commentC);
        Assert.NotNull(requestorReply);

        var recipients = await comments.GetPriorAssigneeCommenterUserIdsAsync(
            created.Id, UserA, requestorReply!.Id);

        Assert.Equal(2, recipients.Count);
        Assert.Contains(UserB, recipients);
        Assert.Contains("user-c-sub", recipients);
    }

    [Fact]
    public async Task GetPriorAssigneeCommenterUserIdsAsync_ReturnsEmpty_WhenNoAssigneeCommentsYet()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var requests = TestDbContextFactory.CreateRequestService(context);
        var comments = TestDbContextFactory.CreateRequestCommentService(context);

        var created = await requests.AddAsync(new Request
        {
            UserId = UserA,
            Title = "No assignee comments",
            RequestTypeId = 2,
        });

        var requestorComment = await comments.AddAsync(created.Id, UserA, "First from requestor");

        Assert.NotNull(requestorComment);
        var recipients = await comments.GetPriorAssigneeCommenterUserIdsAsync(
            created.Id, UserA, requestorComment!.Id);

        Assert.Empty(recipients);
    }
}
