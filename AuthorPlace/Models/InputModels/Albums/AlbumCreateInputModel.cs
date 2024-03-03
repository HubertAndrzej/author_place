using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels.Albums;

public class AlbumCreateInputModel
{
    [Display(Name = "Title")]
    public string? Title { get; set; }
}
