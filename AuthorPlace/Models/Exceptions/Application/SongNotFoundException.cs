namespace AuthorPlace.Models.Exceptions.Application;

public class SongNotFoundException : Exception
{
    public SongNotFoundException(int songId) : base($"Song {songId} not found")
    {
    }
}
