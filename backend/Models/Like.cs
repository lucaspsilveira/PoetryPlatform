namespace PoetryPlatform.Api.Models;

public class Like
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public int PoemId { get; set; }
    public Poem Poem { get; set; } = null!;
}
