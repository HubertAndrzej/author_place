namespace AuthorPlace.Models.Entities;

public class Song
{
    public Song(string title, int albumId)
    {
        ChangeTitle(title);
        AlbumId = albumId;
        Duration = TimeSpan.FromSeconds(0);
    }

    public int Id { get; set; }
    public int AlbumId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TimeSpan Duration { get; set; }
    public string? RowVersion { get; private set; }
    public virtual Album Album { get; set; } = null!;

    public void ChangeTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentException("The song must have a title");
        }
        Title = title;
    }

    public void ChangeDescription(string description)
    {
        Description = description;
    }

    public void ChangeDuration(TimeSpan duration)
    {
        Duration = duration;
    }
}
