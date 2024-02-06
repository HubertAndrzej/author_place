namespace AuthorPlace.Models.Entities;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string Author { get; set; } = null!;
    public string? Email { get; set; }
    public double Rating { get; set; }
    public string FullPriceAmount { get; set; } = null!;
    public string FullPriceCurrency { get; set; } = null!;
    public string CurrentPriceAmount { get; set; } = null!;
    public string CurrentPriceCurrency { get; set; } = null!;
    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
