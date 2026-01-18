using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PoetryPlatform.Api.Controllers;
using PoetryPlatform.Api.DTOs;
using PoetryPlatform.Api.Services;

namespace PoetryPlatform.Api.Tests.Controllers;

public class PoemsControllerLikeTests
{
    private readonly Mock<IPoemService> _mockPoemService;
    private readonly PoemsController _controller;
    private const string TestUserId = "test-user-id";

    public PoemsControllerLikeTests()
    {
        _mockPoemService = new Mock<IPoemService>();
        _controller = new PoemsController(_mockPoemService.Object);
    }

    private void SetupAuthenticatedUser(string? userId = TestUserId)
    {
        var claims = new List<Claim>();
        if (userId != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static PoemResponse CreateTestPoemResponse(int likeCount = 0, bool isLikedByCurrentUser = false)
    {
        return new PoemResponse(
            Id: 1,
            Title: "Test Poem",
            Content: "Test content",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsPublished: true,
            Author: new AuthorDto("author-id", "Test Author"),
            LikeCount: likeCount,
            IsLikedByCurrentUser: isLikedByCurrentUser
        );
    }

    [Fact]
    public async Task Like_WhenAuthenticated_ReturnsOkWithUpdatedPoem()
    {
        // Arrange
        SetupAuthenticatedUser();
        var expectedPoem = CreateTestPoemResponse(likeCount: 1, isLikedByCurrentUser: true);
        _mockPoemService.Setup(s => s.LikeAsync(1, TestUserId))
            .ReturnsAsync(expectedPoem);

        // Act
        var result = await _controller.Like(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var poem = Assert.IsType<PoemResponse>(okResult.Value);
        Assert.Equal(1, poem.LikeCount);
        Assert.True(poem.IsLikedByCurrentUser);
        _mockPoemService.Verify(s => s.LikeAsync(1, TestUserId), Times.Once);
    }

    [Fact]
    public async Task Like_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        SetupAuthenticatedUser(null);

        // Act
        var result = await _controller.Like(1);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockPoemService.Verify(s => s.LikeAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Like_WhenPoemNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupAuthenticatedUser();
        _mockPoemService.Setup(s => s.LikeAsync(999, TestUserId))
            .ReturnsAsync((PoemResponse?)null);

        // Act
        var result = await _controller.Like(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Unlike_WhenAuthenticated_ReturnsOkWithUpdatedPoem()
    {
        // Arrange
        SetupAuthenticatedUser();
        var expectedPoem = CreateTestPoemResponse(likeCount: 0, isLikedByCurrentUser: false);
        _mockPoemService.Setup(s => s.UnlikeAsync(1, TestUserId))
            .ReturnsAsync(expectedPoem);

        // Act
        var result = await _controller.Unlike(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var poem = Assert.IsType<PoemResponse>(okResult.Value);
        Assert.Equal(0, poem.LikeCount);
        Assert.False(poem.IsLikedByCurrentUser);
        _mockPoemService.Verify(s => s.UnlikeAsync(1, TestUserId), Times.Once);
    }

    [Fact]
    public async Task Unlike_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        SetupAuthenticatedUser(null);

        // Act
        var result = await _controller.Unlike(1);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockPoemService.Verify(s => s.UnlikeAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Unlike_WhenPoemNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupAuthenticatedUser();
        _mockPoemService.Setup(s => s.UnlikeAsync(999, TestUserId))
            .ReturnsAsync((PoemResponse?)null);

        // Act
        var result = await _controller.Unlike(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetFeed_PassesCurrentUserIdToService()
    {
        // Arrange
        SetupAuthenticatedUser();
        var poems = new PoemListResponse(
            new List<PoemResponse> { CreateTestPoemResponse(likeCount: 5, isLikedByCurrentUser: true) },
            TotalCount: 1,
            Page: 1,
            PageSize: 10
        );
        _mockPoemService.Setup(s => s.GetFeedAsync(1, 10, TestUserId))
            .ReturnsAsync(poems);

        // Act
        var result = await _controller.GetFeed(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PoemListResponse>(okResult.Value);
        Assert.Single(response.Poems);
        Assert.Equal(5, response.Poems.First().LikeCount);
        Assert.True(response.Poems.First().IsLikedByCurrentUser);
        _mockPoemService.Verify(s => s.GetFeedAsync(1, 10, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetFeed_WhenNotAuthenticated_PassesNullUserId()
    {
        // Arrange
        SetupAuthenticatedUser(null);
        var poems = new PoemListResponse(
            new List<PoemResponse> { CreateTestPoemResponse(likeCount: 5, isLikedByCurrentUser: false) },
            TotalCount: 1,
            Page: 1,
            PageSize: 10
        );
        _mockPoemService.Setup(s => s.GetFeedAsync(1, 10, null))
            .ReturnsAsync(poems);

        // Act
        var result = await _controller.GetFeed(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PoemListResponse>(okResult.Value);
        Assert.False(response.Poems.First().IsLikedByCurrentUser);
        _mockPoemService.Verify(s => s.GetFeedAsync(1, 10, null), Times.Once);
    }

    [Fact]
    public async Task GetById_PassesCurrentUserIdToService()
    {
        // Arrange
        SetupAuthenticatedUser();
        var poem = CreateTestPoemResponse(likeCount: 3, isLikedByCurrentUser: true);
        _mockPoemService.Setup(s => s.GetByIdAsync(1, TestUserId))
            .ReturnsAsync(poem);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PoemResponse>(okResult.Value);
        Assert.Equal(3, response.LikeCount);
        Assert.True(response.IsLikedByCurrentUser);
        _mockPoemService.Verify(s => s.GetByIdAsync(1, TestUserId), Times.Once);
    }
}
