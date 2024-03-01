namespace AuthorPlace.Models.Exceptions.Application;

public class AlbumNotFoundException : Exception
{
    public AlbumNotFoundException(int albumId) : base($"Album {albumId} not found")
    {
        AlbumId = albumId;
    }

    public int AlbumId { get; }
}
