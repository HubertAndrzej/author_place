namespace AuthorPlace.Models.Exceptions.Application;

public class AlbumSubscriptionNotFoundException : Exception
{
    public AlbumSubscriptionNotFoundException(int albumId) : base($"Subscription to album {albumId} not found")
    {
    }
}
