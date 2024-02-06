namespace AuthorPlace.Models.Entities;

public class Song
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Duration { get; set; } = null!;
    public virtual Album Album { get; set; } = null!;
}
