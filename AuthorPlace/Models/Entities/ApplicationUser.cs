using Microsoft.AspNetCore.Identity;

namespace AuthorPlace.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public virtual ICollection<Album> AuthoredAlbums { get; private set; } = new List<Album>();
    public virtual ICollection<Album> SubscribedAlbums { get; private set; } = new List<Album>();
}
