using System.ComponentModel.DataAnnotations;

namespace PoetryPlatform.Api.DTOs;

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    [Required][MaxLength(100)] string DisplayName
);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string DisplayName
);

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt
);
