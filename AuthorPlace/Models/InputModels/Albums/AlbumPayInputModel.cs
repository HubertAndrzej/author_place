using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.InputModels.Albums;

public class AlbumPayInputModel
{
    public string? UserId { get; set; }
    public int AlbumId { get; set; }
    public string? Description { get; set; }
    public Money? Price { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}
