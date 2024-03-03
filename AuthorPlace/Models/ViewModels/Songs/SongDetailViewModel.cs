namespace AuthorPlace.Models.ViewModels.Songs;

public class SongDetailViewModel
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public string? Title { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Description { get; set; }
}
