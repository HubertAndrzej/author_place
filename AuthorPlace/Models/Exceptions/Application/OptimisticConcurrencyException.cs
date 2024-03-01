namespace AuthorPlace.Models.Exceptions.Application;

public class OptimisticConcurrencyException : Exception
{
    public OptimisticConcurrencyException() : base($"The update failed because another user updated the values ​​in the meantime")
    {
    }
}
