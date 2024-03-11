namespace AuthorPlace.Models.Exceptions.Application;

public class UserUnknownException : Exception
{
    public UserUnknownException() : base($"An authenticated user is required for this operation")
    {
    }
}
