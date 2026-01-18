using Microsoft.EntityFrameworkCore;
using PoetryPlatform.Api.Data;
using PoetryPlatform.Api.Models;
using PoetryPlatform.Api.Services;

namespace PoetryPlatform.Api.Tests.Services;

public class PoemServiceLikeTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PoemService _service;
    private readonly User _testUser;
    private readonly User _anotherUser;
    private readonly Poem _testPoem;

    public PoemServiceLikeTests()
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
            DisplayName = "Test User"
        };

        _anotherUser = new User
        {
            Id = "user-2",
            UserName = "another@example.com",
            Email = "another@example.com",
            DisplayName = "Another User"
        };

        _context.Users.AddRange(_testUser, _anotherUser);

        _testPoem = new Poem
        {
            Id = 1,
            Title = "Test Poem",
            Content = "This is a test poem content",
            UserId = _testUser.Id,
            User = _testUser,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Poems.Add(_testPoem);
        _context.SaveChanges();

        _service = new PoemService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task LikeAsync_WhenPoemExists_AddsLikeAndReturnsUpdatedPoem()
    {
        // Act
        var result = await _service.LikeAsync(_testPoem.Id, _anotherUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.LikeCount);
        Assert.True(result.IsLikedByCurrentUser);

        var likeInDb = await _context.Likes.FirstOrDefaultAsync(l => l.PoemId == _testPoem.Id && l.UserId == _anotherUser.Id);
        Assert.NotNull(likeInDb);
    }

    [Fact]
    public async Task LikeAsync_WhenPoemDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _service.LikeAsync(999, _anotherUser.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LikeAsync_WhenAlreadyLiked_DoesNotDuplicateLike()
    {
        // Arrange
        var existingLike = new Like
        {
            UserId = _anotherUser.Id,
            PoemId = _testPoem.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Likes.Add(existingLike);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.LikeAsync(_testPoem.Id, _anotherUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.LikeCount);

        var likesCount = await _context.Likes.CountAsync(l => l.PoemId == _testPoem.Id && l.UserId == _anotherUser.Id);
        Assert.Equal(1, likesCount);
    }

    [Fact]
    public async Task LikeAsync_MultipleDifferentUsers_IncreasesLikeCount()
    {
        // Act
        await _service.LikeAsync(_testPoem.Id, _testUser.Id);
        var result = await _service.LikeAsync(_testPoem.Id, _anotherUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.LikeCount);
    }

    [Fact]
    public async Task UnlikeAsync_WhenPoemExists_RemovesLikeAndReturnsUpdatedPoem()
    {
        // Arrange
        var existingLike = new Like
        {
            UserId = _anotherUser.Id,
            PoemId = _testPoem.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Likes.Add(existingLike);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UnlikeAsync(_testPoem.Id, _anotherUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.LikeCount);
        Assert.False(result.IsLikedByCurrentUser);

        var likeInDb = await _context.Likes.FirstOrDefaultAsync(l => l.PoemId == _testPoem.Id && l.UserId == _anotherUser.Id);
        Assert.Null(likeInDb);
    }

    [Fact]
    public async Task UnlikeAsync_WhenPoemDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _service.UnlikeAsync(999, _anotherUser.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UnlikeAsync_WhenNotLiked_ReturnsPoem()
    {
        // Act
        var result = await _service.UnlikeAsync(_testPoem.Id, _anotherUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.LikeCount);
        Assert.False(result.IsLikedByCurrentUser);
    }

    [Fact]
    public async Task UnlikeAsync_OnlyRemovesOwnLike()
    {
        // Arrange
        var like1 = new Like { UserId = _testUser.Id, PoemId = _testPoem.Id, CreatedAt = DateTime.UtcNow };
        var like2 = new Like { UserId = _anotherUser.Id, PoemId = _testPoem.Id, CreatedAt = DateTime.UtcNow };
        _context.Likes.AddRange(like1, like2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UnlikeAsync(_testPoem.Id, _anotherUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.LikeCount);

        var remainingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PoemId == _testPoem.Id);
        Assert.NotNull(remainingLike);
        Assert.Equal(_testUser.Id, remainingLike.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_IncludesLikeCountAndIsLikedStatus()
    {
        // Arrange
        var like = new Like { UserId = _anotherUser.Id, PoemId = _testPoem.Id, CreatedAt = DateTime.UtcNow };
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        var resultForLiker = await _service.GetByIdAsync(_testPoem.Id, _anotherUser.Id);
        var resultForNonLiker = await _service.GetByIdAsync(_testPoem.Id, _testUser.Id);
        var resultForAnonymous = await _service.GetByIdAsync(_testPoem.Id, null);

        // Assert
        Assert.NotNull(resultForLiker);
        Assert.Equal(1, resultForLiker.LikeCount);
        Assert.True(resultForLiker.IsLikedByCurrentUser);

        Assert.NotNull(resultForNonLiker);
        Assert.Equal(1, resultForNonLiker.LikeCount);
        Assert.False(resultForNonLiker.IsLikedByCurrentUser);

        Assert.NotNull(resultForAnonymous);
        Assert.Equal(1, resultForAnonymous.LikeCount);
        Assert.False(resultForAnonymous.IsLikedByCurrentUser);
    }

    [Fact]
    public async Task GetFeedAsync_IncludesLikeCountAndIsLikedStatus()
    {
        // Arrange
        var like = new Like { UserId = _anotherUser.Id, PoemId = _testPoem.Id, CreatedAt = DateTime.UtcNow };
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        var resultForLiker = await _service.GetFeedAsync(1, 10, _anotherUser.Id);
        var resultForNonLiker = await _service.GetFeedAsync(1, 10, _testUser.Id);

        // Assert
        var poemForLiker = resultForLiker.Poems.First();
        Assert.Equal(1, poemForLiker.LikeCount);
        Assert.True(poemForLiker.IsLikedByCurrentUser);

        var poemForNonLiker = resultForNonLiker.Poems.First();
        Assert.Equal(1, poemForNonLiker.LikeCount);
        Assert.False(poemForNonLiker.IsLikedByCurrentUser);
    }
}
