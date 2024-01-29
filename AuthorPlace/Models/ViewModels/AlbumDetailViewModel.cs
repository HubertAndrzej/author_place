namespace AuthorPlace.Models.ViewModels;

public class AlbumDetailViewModel : AlbumViewModel
{
    public string? Description { get; set; }
    public List<SongViewModel>? Songs { get; set; }

    public TimeSpan TotalAlbumDuration
    {
        get => TimeSpan.FromSeconds(Songs!.Sum(x => x.Duration.TotalSeconds));
    }
}
