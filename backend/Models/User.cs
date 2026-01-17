using Microsoft.AspNetCore.Identity;

namespace PoetryPlatform.Api.Models;

public class User : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Poem> Poems { get; set; } = new List<Poem>();
}
