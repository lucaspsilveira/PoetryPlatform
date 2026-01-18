using System.ComponentModel.DataAnnotations;

namespace PoetryPlatform.Api.DTOs;

public record CreatePoemRequest(
    [Required][MaxLength(200)] string Title,
    [Required] string Content,
    bool IsPublished = true
);

public record UpdatePoemRequest(
    [MaxLength(200)] string? Title,
    string? Content,
    bool? IsPublished
);

public record PoemResponse(
    int Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsPublished,
    AuthorDto Author,
    int LikeCount,
    bool IsLikedByCurrentUser
);

public record AuthorDto(
    string Id,
    string DisplayName
);

public record PoemListResponse(
    IEnumerable<PoemResponse> Poems,
    int TotalCount,
    int Page,
    int PageSize
);
