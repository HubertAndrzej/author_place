namespace AuthorPlace.Models.Exceptions.Application;

public class AlbumDeletionException : Exception
{
    public AlbumDeletionException(int albumId) : base($"Cannot delete the album {albumId} since it has subscribers")
    {
    }
}