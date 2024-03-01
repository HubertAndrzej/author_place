namespace AuthorPlace.Models.Exceptions.Infrastructure;

public class ConstraintViolationException : Exception
{
    public ConstraintViolationException(Exception exception) : base($"A violation occurred for a database constraint", exception)
    {
    }
}
