namespace AuthorPlace.Models.Exceptions.Application;

public class SendException : Exception
{
    public SendException() : base($"The message could not be sent")
    {
    }
}
