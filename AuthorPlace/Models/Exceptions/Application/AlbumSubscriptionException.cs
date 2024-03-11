namespace AuthorPlace.Models.Exceptions.Application;

public class AlbumSubscriptionException : Exception
{
    public AlbumSubscriptionException(int albumId) : base($"Subriction to album {albumId} failed")
    {     
    }
}
