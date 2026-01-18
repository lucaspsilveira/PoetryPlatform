using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PoetryPlatform.Api.DTOs;
using PoetryPlatform.Api.Services;

namespace PoetryPlatform.Api.Controllers;

/// <summary>
/// Handles user profile operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IPoemService _poemService;

    public UsersController(IPoemService poemService)
    {
        _poemService = poemService;
    }

    /// <summary>
    /// Get a user's public profile with their top 10 poems.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>User profile with display name, join date, and top poems.</returns>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _poemService.GetUserProfileAsync(id, currentUserId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    /// <summary>
    /// Get all published poems by a user (paginated).
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of the user's published poems.</returns>
    /// <response code="200">Returns the paginated list of poems.</response>
    [HttpGet("{id}/poems")]
    [ProducesResponseType(typeof(PoemListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PoemListResponse>> GetUserPoems(
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _poemService.GetPublicUserPoemsAsync(id, page, pageSize, currentUserId);
        return Ok(result);
    }
}
