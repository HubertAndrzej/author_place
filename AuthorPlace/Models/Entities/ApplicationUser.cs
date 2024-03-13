using Microsoft.AspNetCore.Identity;

namespace AuthorPlace.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string? FullName { get; set; }

    [PersonalData]
    public DateTimeOffset EcommerceConsent { get; set; }

    [PersonalData]
    public DateTimeOffset? NewsletterConsent { get; set; }

    public virtual ICollection<Album> AuthoredAlbums { get; private set; } = new List<Album>();

    public virtual ICollection<Album> SubscribedAlbums { get; private set; } = new List<Album>();
}
