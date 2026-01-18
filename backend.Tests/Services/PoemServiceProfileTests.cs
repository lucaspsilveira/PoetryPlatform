using Microsoft.EntityFrameworkCore;
using PoetryPlatform.Api.Data;
using PoetryPlatform.Api.Models;
using PoetryPlatform.Api.Services;

namespace PoetryPlatform.Api.Tests.Services;

public class PoemServiceProfileTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PoemService _service;
    private readonly User _testUser;
    private readonly User _anotherUser;

    public PoemServiceProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _testUser = new User
        {
            Id = "user-1",
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            DisplayName = "Test User",
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        _anotherUser = new User
        {
            Id = "user-2",
            UserName = "another@example.com",
            Email = "another@example.com",
            DisplayName = "Another User",
            CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        _context.Users.AddRange(_testUser, _anotherUser);
        _context.SaveChanges();

        _service = new PoemService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private Poem CreatePoem(string userId, string title, bool isPublished = true, int likeCount = 0)
    {
        var poem = new Poem
        {
            Title = title,
            Content = $"Content of {title}",
            UserId = userId,
            User = userId == _testUser.Id ? _testUser : _anotherUser,
            IsPublished = isPublished,
            CreatedAt = DateTime.UtcNow
        };

        _context.Poems.Add(poem);
        _context.SaveChanges();

        // Add likes
        for (int i = 0; i < likeCount; i++)
        {
            var like = new Like
            {
                UserId = $"liker-{i}",
                PoemId = poem.Id,
                CreatedAt = DateTime.UtcNow
            };
            poem.Likes.Add(like);
        }
        _context.SaveChanges();

        return poem;
    }

    #region GetUserProfileAsync Tests

    [Fact]
    public async Task GetUserProfileAsync_WhenUserExists_ReturnsProfile()
    {
        // Arrange
        CreatePoem(_testUser.Id, "Poem 1", isPublished: true);
        CreatePoem(_testUser.Id, "Poem 2", isPublished: true);

        // Act
        var result = await _service.GetUserProfileAsync(_testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testUser.Id, result.Id);
        Assert.Equal(_testUser.DisplayName, result.DisplayName);
        Assert.Equal(_testUser.CreatedAt, result.CreatedAt);
        Assert.Equal(2, result.TotalPoemCount);
        Assert.Equal(2, result.TopPoems.Count());
    }

    [Fact]
    public async Task GetUserProfileAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserProfileAsync("non-existent-user");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserProfileAsync_OnlyCountsPublishedPoems()
    {
        // Arrange
        CreatePoem(_testUser.Id, "Published Poem", isPublished: true);
        CreatePoem(_testUser.Id, "Draft Poem", isPublished: false);

        // Act
        var result = await _service.GetUserProfileAsync(_testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalPoemCount);
        Assert.Single(result.TopPoems);
    }

    [Fact]
    public async Task GetUserProfileAsync_ReturnsTop10PoemsByLikeCount()
    {
        // Arrange - Create 12 poems with varying like counts
        for (int i = 1; i <= 12; i++)
        {
            CreatePoem(_testUser.Id, $"Poem {i}", isPublished: true, likeCount: i);
        }

        // Act
        var result = await _service.GetUserProfileAsync(_testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.TotalPoemCount);
        Assert.Equal(10, result.TopPoems.Count());

        // Verify ordering - highest likes first
        var likeCounts = result.TopPoems.Select(p => p.LikeCount).ToList();
        Assert.Equal(likeCounts.OrderByDescending(x => x), likeCounts);

        // Top poem should have 12 likes
        Assert.Equal(12, result.TopPoems.First().LikeCount);
    }

    [Fact]
    public async Task GetUserProfileAsync_WhenUserHasNoPoems_ReturnsEmptyTopPoems()
    {
        // Act
        var result = await _service.GetUserProfileAsync(_testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPoemCount);
        Assert.Empty(result.TopPoems);
    }

    [Fact]
    public async Task GetUserProfileAsync_IncludesIsLikedByCurrentUser()
    {
        // Arrange
        var poem = CreatePoem(_testUser.Id, "Liked Poem", isPublished: true);
        var like = new Like
        {
            UserId = _anotherUser.Id,
            PoemId = poem.Id,
            CreatedAt = DateTime.UtcNow
        };
        poem.Likes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        var resultForLiker = await _service.GetUserProfileAsync(_testUser.Id, _anotherUser.Id);
        var resultForNonLiker = await _service.GetUserProfileAsync(_testUser.Id, _testUser.Id);
        var resultForAnonymous = await _service.GetUserProfileAsync(_testUser.Id, null);

        // Assert
        Assert.True(resultForLiker!.TopPoems.First().IsLikedByCurrentUser);
        Assert.False(resultForNonLiker!.TopPoems.First().IsLikedByCurrentUser);
        Assert.False(resultForAnonymous!.TopPoems.First().IsLikedByCurrentUser);
    }

    #endregion

    #region GetPublicUserPoemsAsync Tests

    [Fact]
    public async Task GetPublicUserPoemsAsync_ReturnsPaginatedPoems()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            CreatePoem(_testUser.Id, $"Poem {i}", isPublished: true);
        }

        // Act
        var page1 = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10);
        var page2 = await _service.GetPublicUserPoemsAsync(_testUser.Id, 2, 10);

        // Assert
        Assert.Equal(15, page1.TotalCount);
        Assert.Equal(10, page1.Poems.Count());
        Assert.Equal(1, page1.Page);
        Assert.Equal(10, page1.PageSize);

        Assert.Equal(15, page2.TotalCount);
        Assert.Equal(5, page2.Poems.Count());
        Assert.Equal(2, page2.Page);
    }

    [Fact]
    public async Task GetPublicUserPoemsAsync_OnlyReturnsPublishedPoems()
    {
        // Arrange
        CreatePoem(_testUser.Id, "Published 1", isPublished: true);
        CreatePoem(_testUser.Id, "Published 2", isPublished: true);
        CreatePoem(_testUser.Id, "Draft 1", isPublished: false);

        // Act
        var result = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Poems.Count());
        Assert.All(result.Poems, p => Assert.True(p.IsPublished));
    }

    [Fact]
    public async Task GetPublicUserPoemsAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var poem1 = CreatePoem(_testUser.Id, "Oldest Poem", isPublished: true);
        poem1.CreatedAt = DateTime.UtcNow.AddDays(-2);

        var poem2 = CreatePoem(_testUser.Id, "Middle Poem", isPublished: true);
        poem2.CreatedAt = DateTime.UtcNow.AddDays(-1);

        var poem3 = CreatePoem(_testUser.Id, "Newest Poem", isPublished: true);
        poem3.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10);

        // Assert
        var poems = result.Poems.ToList();
        Assert.Equal("Newest Poem", poems[0].Title);
        Assert.Equal("Middle Poem", poems[1].Title);
        Assert.Equal("Oldest Poem", poems[2].Title);
    }

    [Fact]
    public async Task GetPublicUserPoemsAsync_IncludesLikeCount()
    {
        // Arrange
        CreatePoem(_testUser.Id, "Popular Poem", isPublished: true, likeCount: 5);

        // Act
        var result = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10);

        // Assert
        Assert.Equal(5, result.Poems.First().LikeCount);
    }

    [Fact]
    public async Task GetPublicUserPoemsAsync_IncludesIsLikedByCurrentUser()
    {
        // Arrange
        var poem = CreatePoem(_testUser.Id, "Poem", isPublished: true);
        var like = new Like
        {
            UserId = _anotherUser.Id,
            PoemId = poem.Id,
            CreatedAt = DateTime.UtcNow
        };
        poem.Likes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        var resultForLiker = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10, _anotherUser.Id);
        var resultForNonLiker = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10, _testUser.Id);

        // Assert
        Assert.True(resultForLiker.Poems.First().IsLikedByCurrentUser);
        Assert.False(resultForNonLiker.Poems.First().IsLikedByCurrentUser);
    }

    [Fact]
    public async Task GetPublicUserPoemsAsync_ReturnsEmptyWhenUserHasNoPublishedPoems()
    {
        // Arrange
        CreatePoem(_testUser.Id, "Draft", isPublished: false);

        // Act
        var result = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10);

        // Assert
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Poems);
    }

    [Fact]
    public async Task GetPublicUserPoemsAsync_OnlyReturnsSpecifiedUserPoems()
    {
        // Arrange
        CreatePoem(_testUser.Id, "User 1 Poem", isPublished: true);
        CreatePoem(_anotherUser.Id, "User 2 Poem", isPublished: true);

        // Act
        var result = await _service.GetPublicUserPoemsAsync(_testUser.Id, 1, 10);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("User 1 Poem", result.Poems.First().Title);
    }

    #endregion
}
