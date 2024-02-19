namespace AuthorPlace.Models.ViewModels;

public class HomeViewModel
{
    public List<AlbumViewModel>? BestRatingAlbums { get; set; }
    public List<AlbumViewModel>? MostRecentAlbums { get; set; }
}
