using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels.Songs;

public class SongCreateInputModel
{
    public int AlbumId { get; set; }

    [Display(Name = "Title")]
    public string? Title { get; set; }
}
