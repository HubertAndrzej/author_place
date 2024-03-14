using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Exceptions.Infrastructure;
using AuthorPlace.Models.Services.Application.Interfaces.Errors;
using AuthorPlace.Models.ValueObjects;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace AuthorPlace.Models.Services.Application.Implementations.Errors;

public class ErrorViewSelectorService : IErrorViewSelectorService
{
    public ErrorViewData GetErrorViewData(HttpContext context)
    {
        Exception? exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        return exception switch
        {
            null => new ErrorViewData(
                title: $"Page not found",
                statusCode: HttpStatusCode.NotFound,
                viewName: "NotFound"),

            AlbumNotFoundException e => new ErrorViewData(
                title: $"Album {e.AlbumId} not found",
                statusCode: HttpStatusCode.NotFound,
                viewName: "AlbumNotFound"),

            AlbumDeletionException => new ErrorViewData(
                title: $"Cannot delete the album since it has subscribers",
                statusCode: HttpStatusCode.Forbidden,
                viewName: "AlbumWithSubscribers"),

            UserUnknownException => new ErrorViewData(
                title: "User not found",
                statusCode: HttpStatusCode.BadRequest,
                viewName: "Index"),

            AlbumSubscriptionException => new ErrorViewData(
                title: "The subscription to the album failed",
                statusCode: HttpStatusCode.BadRequest,
                viewName: "Index"),

            AlbumSubscriptionNotFoundException => new ErrorViewData(
                title: "Subscription not found",
                statusCode: HttpStatusCode.BadRequest,
                viewName: "Index"),

            PaymentGatewayException => new ErrorViewData(
                title: $"Something went wrong with the payment",
                statusCode: HttpStatusCode.InternalServerError,
                viewName: "Index"),

            SendException => new ErrorViewData(
                title: $"The message could not be sent",
                statusCode: HttpStatusCode.InternalServerError,
                viewName: "Index"),

            _ => new ErrorViewData(
                title: "Error",
                statusCode: HttpStatusCode.BadRequest,
                viewName: "Index")
        };
    }
}
