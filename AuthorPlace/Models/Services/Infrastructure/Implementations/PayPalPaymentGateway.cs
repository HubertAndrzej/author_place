using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlaceMoney = AuthorPlace.Models.ValueObjects.Money;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalResponse = PayPalHttp.HttpResponse;
using System.Globalization;
using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Infrastructure;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class PayPalPaymentGateway : IPaymentGateway
{
    private readonly IOptionsMonitor<PayPalOptions> options;

    public PayPalPaymentGateway(IOptionsMonitor<PayPalOptions> options)
    {
        this.options = options;
    }

    public async Task<string> GetPaymentUrlAsync(AlbumPayInputModel inputModel)
    {
        OrderRequest order = new()
        {
            CheckoutPaymentIntent = "CAPTURE",
            ApplicationContext = new ApplicationContext()
            {
                ReturnUrl = inputModel.ReturnUrl,
                CancelUrl = inputModel.CancelUrl,
                BrandName = options.CurrentValue.BrandName,
                ShippingPreference = "NO_SHIPPING"
            },
            PurchaseUnits = new List<PurchaseUnitRequest>()
            {
                new()
                {
                    CustomId = $"{inputModel.AlbumId}/{inputModel.UserId}",
                    Description = inputModel.Description,
                    AmountWithBreakdown = new AmountWithBreakdown()
                    {
                        CurrencyCode = inputModel.Price!.Currency.ToString(),
                        Value = inputModel.Price!.Amount.ToString(CultureInfo.InvariantCulture)
                    }
                }
            }
        };
        PayPalEnvironment environment = GetPayPalEnvironment(options.CurrentValue);
        PayPalHttpClient client = new(environment);
        OrdersCreateRequest request = new();
        request.RequestBody(order);
        request.Prefer("return=representation");
        PayPalResponse response = await client.Execute(request);
        Order result = response.Result<Order>();
        LinkDescription link = result.Links.Single(link => link.Rel == "approve");
        return link.Href;
    }

    public async Task<AlbumSubscribeInputModel> CapturePaymentAsync(string token)
    {
        PayPalEnvironment environment = GetPayPalEnvironment(options.CurrentValue);
        PayPalHttpClient client = new(environment);
        OrdersCaptureRequest request = new(token);
        request.RequestBody(new OrderActionRequest());
        request.Prefer("return=representation");
        try
        {
            PayPalResponse response = await client.Execute(request);
            Order result = response.Result<Order>();
            PurchaseUnit purchaseUnit = result.PurchaseUnits.First();
            Capture capture = purchaseUnit.Payments.Captures.First();
            string[] customId = purchaseUnit.CustomId.Split('/');
            int albumId = int.Parse(customId[0]);
            string userId = customId[1];
            AuthorPlaceMoney paid = new(Enum.Parse<Currency>(capture.Amount.CurrencyCode), decimal.Parse(capture.Amount.Value, CultureInfo.InvariantCulture));
            DateTime paymentDate = DateTime.Parse(capture.CreateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            AlbumSubscribeInputModel inputModel = new()
            {
                AlbumId = albumId,
                UserId = userId,
                Paid = paid,
                TransactionId = capture.Id,
                PaymentDate = paymentDate,
                PaymentType = PaymentType.PayPal.ToString()
            };
            return inputModel;
        }
        catch (Exception exception)
        {
            throw new PaymentGatewayException(exception);
        }
    }

    private static PayPalEnvironment GetPayPalEnvironment(PayPalOptions options)
    {
        string clientId = options.ClientId!;
        string secret = options.Secret!;
        return options.IsSandbox ? new SandboxEnvironment(clientId, secret) : new LiveEnvironment(clientId, secret);
    }
}
