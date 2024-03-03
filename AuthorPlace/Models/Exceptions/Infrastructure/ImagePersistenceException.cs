namespace AuthorPlace.Models.Exceptions.Infrastructure;

public class ImagePersistenceException : Exception
{
    public ImagePersistenceException(Exception exception) : base("Couldn't persist the image", exception)
    {
    }
}
