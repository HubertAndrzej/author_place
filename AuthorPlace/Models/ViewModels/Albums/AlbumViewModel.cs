using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.ViewModels.Albums;

public class AlbumViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ImagePath { get; set; }
    public string? Author { get; set; }
    public string? AuthorId { get; set; }
    public double Rating { get; set; }
    public Money? FullPrice { get; set; }
    public Money? CurrentPrice { get; set; }
    public Status Status { get; set; }
}
