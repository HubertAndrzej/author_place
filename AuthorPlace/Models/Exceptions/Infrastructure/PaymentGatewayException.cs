namespace AuthorPlace.Models.Exceptions.Infrastructure;

public class PaymentGatewayException : Exception
{
    public PaymentGatewayException(Exception exception) : base($"Payment gateway threw an exception", exception)
    {
    }
}
