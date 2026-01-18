using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PoetryPlatform.Api.Controllers;
using PoetryPlatform.Api.DTOs;
using PoetryPlatform.Api.Services;

namespace PoetryPlatform.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IPoemService> _mockPoemService;
    private readonly UsersController _controller;
    private const string TestUserId = "test-user-id";
    private const string ProfileUserId = "profile-user-id";

    public UsersControllerTests()
    {
        _mockPoemService = new Mock<IPoemService>();
        _controller = new UsersController(_mockPoemService.Object);
    }

    private void SetupUser(string? userId = TestUserId)
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

    private static UserProfileResponse CreateTestProfile()
    {
        return new UserProfileResponse(
            Id: ProfileUserId,
            DisplayName: "Test Poet",
            CreatedAt: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalPoemCount: 5,
            TopPoems: new List<PoemResponse>
            {
                new PoemResponse(
                    Id: 1,
                    Title: "Top Poem",
                    Content: "Content",
                    CreatedAt: DateTime.UtcNow,
                    UpdatedAt: null,
                    IsPublished: true,
                    Author: new AuthorDto(ProfileUserId, "Test Poet"),
                    LikeCount: 10,
                    IsLikedByCurrentUser: false
                )
            }
        );
    }

    private static PoemListResponse CreateTestPoemList()
    {
        return new PoemListResponse(
            Poems: new List<PoemResponse>
            {
                new PoemResponse(
                    Id: 1,
                    Title: "Poem 1",
                    Content: "Content 1",
                    CreatedAt: DateTime.UtcNow,
                    UpdatedAt: null,
                    IsPublished: true,
                    Author: new AuthorDto(ProfileUserId, "Test Poet"),
                    LikeCount: 5,
                    IsLikedByCurrentUser: false
                ),
                new PoemResponse(
                    Id: 2,
                    Title: "Poem 2",
                    Content: "Content 2",
                    CreatedAt: DateTime.UtcNow,
                    UpdatedAt: null,
                    IsPublished: true,
                    Author: new AuthorDto(ProfileUserId, "Test Poet"),
                    LikeCount: 3,
                    IsLikedByCurrentUser: true
                )
            },
            TotalCount: 2,
            Page: 1,
            PageSize: 10
        );
    }

    #region GetProfile Tests

    [Fact]
    public async Task GetProfile_WhenUserExists_ReturnsOkWithProfile()
    {
        // Arrange
        SetupUser();
        var profile = CreateTestProfile();
        _mockPoemService.Setup(s => s.GetUserProfileAsync(ProfileUserId, TestUserId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetProfile(ProfileUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProfile = Assert.IsType<UserProfileResponse>(okResult.Value);
        Assert.Equal(ProfileUserId, returnedProfile.Id);
        Assert.Equal("Test Poet", returnedProfile.DisplayName);
        Assert.Equal(5, returnedProfile.TotalPoemCount);
        _mockPoemService.Verify(s => s.GetUserProfileAsync(ProfileUserId, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetProfile_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        SetupUser();
        _mockPoemService.Setup(s => s.GetUserProfileAsync("non-existent", TestUserId))
            .ReturnsAsync((UserProfileResponse?)null);

        // Act
        var result = await _controller.GetProfile("non-existent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProfile_WhenNotAuthenticated_PassesNullCurrentUserId()
    {
        // Arrange
        SetupUser(null);
        var profile = CreateTestProfile();
        _mockPoemService.Setup(s => s.GetUserProfileAsync(ProfileUserId, null))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetProfile(ProfileUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mockPoemService.Verify(s => s.GetUserProfileAsync(ProfileUserId, null), Times.Once);
    }

    [Fact]
    public async Task GetProfile_IncludesTopPoemsInResponse()
    {
        // Arrange
        SetupUser();
        var profile = CreateTestProfile();
        _mockPoemService.Setup(s => s.GetUserProfileAsync(ProfileUserId, TestUserId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetProfile(ProfileUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProfile = Assert.IsType<UserProfileResponse>(okResult.Value);
        Assert.Single(returnedProfile.TopPoems);
        Assert.Equal("Top Poem", returnedProfile.TopPoems.First().Title);
        Assert.Equal(10, returnedProfile.TopPoems.First().LikeCount);
    }

    #endregion

    #region GetUserPoems Tests

    [Fact]
    public async Task GetUserPoems_ReturnsPaginatedPoems()
    {
        // Arrange
        SetupUser();
        var poemList = CreateTestPoemList();
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<PoemListResponse>(okResult.Value);
        Assert.Equal(2, returnedList.Poems.Count());
        Assert.Equal(2, returnedList.TotalCount);
        Assert.Equal(1, returnedList.Page);
        Assert.Equal(10, returnedList.PageSize);
    }

    [Fact]
    public async Task GetUserPoems_WithCustomPagination_PassesCorrectParameters()
    {
        // Arrange
        SetupUser();
        var poemList = new PoemListResponse(
            Poems: new List<PoemResponse>(),
            TotalCount: 25,
            Page: 3,
            PageSize: 5
        );
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 3, 5, TestUserId))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, 3, 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mockPoemService.Verify(s => s.GetPublicUserPoemsAsync(ProfileUserId, 3, 5, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserPoems_WhenNotAuthenticated_PassesNullCurrentUserId()
    {
        // Arrange
        SetupUser(null);
        var poemList = CreateTestPoemList();
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, null))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mockPoemService.Verify(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, null), Times.Once);
    }

    [Fact]
    public async Task GetUserPoems_WithInvalidPage_DefaultsToPage1()
    {
        // Arrange
        SetupUser();
        var poemList = CreateTestPoemList();
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, -1, 10);

        // Assert
        _mockPoemService.Verify(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserPoems_WithInvalidPageSize_DefaultsTo10()
    {
        // Arrange
        SetupUser();
        var poemList = CreateTestPoemList();
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, 1, 100);

        // Assert
        _mockPoemService.Verify(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserPoems_WithZeroPageSize_DefaultsTo10()
    {
        // Arrange
        SetupUser();
        var poemList = CreateTestPoemList();
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, 1, 0);

        // Assert
        _mockPoemService.Verify(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserPoems_IncludesIsLikedByCurrentUserInResponse()
    {
        // Arrange
        SetupUser();
        var poemList = CreateTestPoemList();
        _mockPoemService.Setup(s => s.GetPublicUserPoemsAsync(ProfileUserId, 1, 10, TestUserId))
            .ReturnsAsync(poemList);

        // Act
        var result = await _controller.GetUserPoems(ProfileUserId, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<PoemListResponse>(okResult.Value);
        var poems = returnedList.Poems.ToList();
        Assert.False(poems[0].IsLikedByCurrentUser);
        Assert.True(poems[1].IsLikedByCurrentUser);
    }

    #endregion
}
