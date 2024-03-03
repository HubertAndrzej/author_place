namespace AuthorPlace.Models.Exceptions.Application;

public class AlbumImageInvalidException : Exception
{
    public AlbumImageInvalidException(int albumId, Exception exception) : base($"Image for album '{albumId}' is not valid", exception)
    {
    }
}
