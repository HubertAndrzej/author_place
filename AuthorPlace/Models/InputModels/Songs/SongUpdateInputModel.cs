using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels.Songs;

public class SongUpdateInputModel
{
    public int Id { get; set; }

    [Display(Name = "Title")]
    public string? Title { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Extimated Duration")]
    public TimeSpan Duration { get; set; }

    public string? RowVersion { get; set; }
}
