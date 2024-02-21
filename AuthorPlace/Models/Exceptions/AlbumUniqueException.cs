namespace AuthorPlace.Models.Exceptions;

public class AlbumUniqueException : Exception
{
    public AlbumUniqueException(string title, string author, Exception exception) : base($"{author} has already an album called {title}", exception)
    {
    }
}
