namespace PoetryPlatform.Api.Models;

public class Poem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = true;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
