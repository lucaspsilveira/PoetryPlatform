using Microsoft.EntityFrameworkCore;
using PoetryPlatform.Api.Data;
using PoetryPlatform.Api.DTOs;
using PoetryPlatform.Api.Models;

namespace PoetryPlatform.Api.Services;

public interface IPoemService
{
    Task<PoemResponse> CreateAsync(string userId, CreatePoemRequest request);
    Task<PoemResponse?> GetByIdAsync(int id);
    Task<PoemListResponse> GetFeedAsync(int page, int pageSize);
    Task<PoemListResponse> GetUserPoemsAsync(string userId, int page, int pageSize);
    Task<PoemResponse?> UpdateAsync(int id, string userId, UpdatePoemRequest request);
    Task<bool> DeleteAsync(int id, string userId);
}

public class PoemService : IPoemService
{
    private readonly ApplicationDbContext _context;

    public PoemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PoemResponse> CreateAsync(string userId, CreatePoemRequest request)
    {
        var poem = new Poem
        {
            Title = request.Title,
            Content = request.Content,
            IsPublished = request.IsPublished,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Poems.Add(poem);
        await _context.SaveChangesAsync();

        await _context.Entry(poem).Reference(p => p.User).LoadAsync();
        return MapToResponse(poem);
    }

    public async Task<PoemResponse?> GetByIdAsync(int id)
    {
        var poem = await _context.Poems
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        return poem == null ? null : MapToResponse(poem);
    }

    public async Task<PoemListResponse> GetFeedAsync(int page, int pageSize)
    {
        var query = _context.Poems
            .Include(p => p.User)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var poems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PoemListResponse(
            poems.Select(MapToResponse),
            totalCount,
            page,
            pageSize
        );
    }

    public async Task<PoemListResponse> GetUserPoemsAsync(string userId, int page, int pageSize)
    {
        var query = _context.Poems
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var poems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PoemListResponse(
            poems.Select(MapToResponse),
            totalCount,
            page,
            pageSize
        );
    }

    public async Task<PoemResponse?> UpdateAsync(int id, string userId, UpdatePoemRequest request)
    {
        var poem = await _context.Poems
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (poem == null) return null;

        if (request.Title != null) poem.Title = request.Title;
        if (request.Content != null) poem.Content = request.Content;
        if (request.IsPublished.HasValue) poem.IsPublished = request.IsPublished.Value;
        poem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(poem);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var poem = await _context.Poems
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (poem == null) return false;

        _context.Poems.Remove(poem);
        await _context.SaveChangesAsync();
        return true;
    }

    private static PoemResponse MapToResponse(Poem poem) => new(
        poem.Id,
        poem.Title,
        poem.Content,
        poem.CreatedAt,
        poem.UpdatedAt,
        poem.IsPublished,
        new AuthorDto(poem.User.Id, poem.User.DisplayName)
    );
}
