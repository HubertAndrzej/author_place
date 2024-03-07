namespace AuthorPlace.Models.Exceptions.Application;

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base($"No user was found for the chosen parameters")
    {
    }
}
