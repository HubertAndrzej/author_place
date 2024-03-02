using AuthorPlace.Models.ViewModels.Albums;

namespace AuthorPlace.Models.ViewModels.Home;

public class HomeViewModel
{
    public List<AlbumViewModel>? BestRatingAlbums { get; set; }
    public List<AlbumViewModel>? MostRecentAlbums { get; set; }
}
