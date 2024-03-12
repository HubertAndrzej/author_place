using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlaceMoney = AuthorPlace.Models.ValueObjects.Money;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Infrastructure;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly IOptionsMonitor<StripeOptions> options;

    public StripePaymentGateway(IOptionsMonitor<StripeOptions> options)
    {
        this.options = options;
    }

    public async Task<string> GetPaymentUrlAsync(AlbumPayInputModel inputModel)
    {
        SessionCreateOptions sessionCreateOptions = new()
        {
            ClientReferenceId = $"{inputModel.AlbumId}/{inputModel.UserId}",
            LineItems = new List<SessionLineItemOptions>() {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = inputModel.Price!.Currency.ToString(),
                        UnitAmount = Convert.ToInt64(inputModel.Price.Amount * 100),
                         ProductData = new SessionLineItemPriceDataProductDataOptions()
                         {
                             Name = inputModel.Description
                         },
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                CaptureMethod = "manual"
            },
            PaymentMethodTypes = new List<string>
            {
                "card"
            },
            SuccessUrl = inputModel.ReturnUrl + "?token={CHECKOUT_SESSION_ID}",
            CancelUrl = inputModel.CancelUrl
        };
        RequestOptions requestOptions = new()
        {
            ApiKey = options.CurrentValue.SecretKey
        };
        SessionService sessionService = new();
        Session session = await sessionService.CreateAsync(sessionCreateOptions, requestOptions);
        return session.Url;
    }

    public async Task<AlbumSubscribeInputModel> CapturePaymentAsync(string token)
    {
        RequestOptions requestOptions = new()
        {
            ApiKey = options.CurrentValue.SecretKey
        };
        try
        {
            SessionService sessionService = new();
            Session session = await sessionService.GetAsync(token, requestOptions: requestOptions);
            PaymentIntentService paymentIntentService = new();
            PaymentIntent paymentIntent = await paymentIntentService.CaptureAsync(session.PaymentIntentId, requestOptions: requestOptions);
            string[] customId = session.ClientReferenceId.Split('/');
            int albumId = int.Parse(customId[0]);
            string userId = customId[1];
            AuthorPlaceMoney paid = new(Enum.Parse<Currency>(paymentIntent.Currency, ignoreCase: true), paymentIntent.Amount / 100m);
            AlbumSubscribeInputModel inputModel = new()
            {
                AlbumId = albumId,
                UserId = userId,
                Paid = paid,
                TransactionId = paymentIntent.Id,
                PaymentDate = paymentIntent.Created,
                PaymentType = PaymentType.Stripe.ToString()
            };
            return inputModel;
        }
        catch (Exception exception)
        {
            throw new PaymentGatewayException(exception);
        }
    }
}
