namespace AuthorPlace.Models.ViewModels;

public class SongViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TimeSpan Duration { get; set; }
}
