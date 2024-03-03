using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels.Songs;

namespace AuthorPlace.Models.ViewModels.Albums;

public class AlbumDetailViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ImagePath { get; set; }
    public string? Author { get; set; }
    public double Rating { get; set; }
    public Money? FullPrice { get; set; }
    public Money? CurrentPrice { get; set; }
    public string? Description { get; set; }
    public List<SongViewModel> Songs { get; set; } = new List<SongViewModel>();

    public TimeSpan TotalAlbumDuration
    {
        get => TimeSpan.FromSeconds(Songs!.Sum(x => x.Duration.TotalSeconds));
    }
}
