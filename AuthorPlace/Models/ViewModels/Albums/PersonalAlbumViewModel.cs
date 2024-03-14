namespace AuthorPlace.Models.ViewModels.Albums;

public class PersonalAlbumViewModel
{
    public List<AlbumViewModel>? AuthoredAlbums { get; set; }
    public List<AlbumViewModel>? SubscribedAlbums { get; set; }
}
