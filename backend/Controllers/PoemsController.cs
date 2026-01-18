using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoetryPlatform.Api.DTOs;
using PoetryPlatform.Api.Services;

namespace PoetryPlatform.Api.Controllers;

/// <summary>
/// Manages poems - create, read, update, delete, and like operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PoemsController : ControllerBase
{
    private readonly IPoemService _poemService;

    public PoemsController(IPoemService poemService)
    {
        _poemService = poemService;
    }

    /// <summary>
    /// Get the public feed of published poems.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of published poems with like counts.</returns>
    /// <response code="200">Returns the paginated list of poems.</response>
    [HttpGet("feed")]
    [ProducesResponseType(typeof(PoemListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PoemListResponse>> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _poemService.GetFeedAsync(page, pageSize, currentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific poem by ID.
    /// </summary>
    /// <param name="id">The poem ID.</param>
    /// <returns>The poem details with like count.</returns>
    /// <response code="200">Returns the poem.</response>
    /// <response code="404">If the poem is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PoemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PoemResponse>> GetById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var poem = await _poemService.GetByIdAsync(id, currentUserId);
        if (poem == null) return NotFound();
        return Ok(poem);
    }

    /// <summary>
    /// Get the authenticated user's poems.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of the user's poems (including drafts).</returns>
    /// <response code="200">Returns the paginated list of user's poems.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [Authorize]
    [HttpGet("my-poems")]
    [ProducesResponseType(typeof(PoemListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PoemListResponse>> GetMyPoems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var result = await _poemService.GetUserPoemsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Create a new poem.
    /// </summary>
    /// <param name="request">The poem content including title, content, and publish status.</param>
    /// <returns>The created poem.</returns>
    /// <response code="201">Returns the newly created poem.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="400">If the request is invalid.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PoemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PoemResponse>> Create([FromBody] CreatePoemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var poem = await _poemService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = poem.Id }, poem);
    }

    /// <summary>
    /// Update an existing poem.
    /// </summary>
    /// <param name="id">The poem ID to update.</param>
    /// <param name="request">The updated poem data.</param>
    /// <returns>The updated poem.</returns>
    /// <response code="200">Returns the updated poem.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the poem is not found or doesn't belong to the user.</response>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PoemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PoemResponse>> Update(int id, [FromBody] UpdatePoemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var poem = await _poemService.UpdateAsync(id, userId, request);
        if (poem == null) return NotFound();
        return Ok(poem);
    }

    /// <summary>
    /// Delete a poem.
    /// </summary>
    /// <param name="id">The poem ID to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The poem was successfully deleted.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the poem is not found or doesn't belong to the user.</response>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _poemService.DeleteAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Like a poem.
    /// </summary>
    /// <param name="id">The poem ID to like.</param>
    /// <returns>The poem with updated like count.</returns>
    /// <response code="200">Returns the poem with updated like status.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the poem is not found.</response>
    [Authorize]
    [HttpPost("{id}/like")]
    [ProducesResponseType(typeof(PoemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PoemResponse>> Like(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var poem = await _poemService.LikeAsync(id, userId);
        if (poem == null) return NotFound();
        return Ok(poem);
    }

    /// <summary>
    /// Unlike a poem.
    /// </summary>
    /// <param name="id">The poem ID to unlike.</param>
    /// <returns>The poem with updated like count.</returns>
    /// <response code="200">Returns the poem with updated like status.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the poem is not found.</response>
    [Authorize]
    [HttpDelete("{id}/like")]
    [ProducesResponseType(typeof(PoemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PoemResponse>> Unlike(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var poem = await _poemService.UnlikeAsync(id, userId);
        if (poem == null) return NotFound();
        return Ok(poem);
    }
}
