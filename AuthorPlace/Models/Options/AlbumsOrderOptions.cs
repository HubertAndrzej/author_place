namespace AuthorPlace.Models.Options;

public class AlbumsOrderOptions
{
    public string? By { get; set; }
    public bool Ascending { get; set; }
    public string[]? Allow { get; set; }
}
