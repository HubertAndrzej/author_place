using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ValueObjects;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class ErrorViewSelectorService : IErrorViewSelectorService
{
    public ErrorViewData GetErrorViewData(HttpContext context)
    {
        Exception? exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        return exception switch
        {
            null => new ErrorViewData(
                title: $"Page '{context.Request.Path}' not found",
                statusCode: HttpStatusCode.NotFound,
                viewName: "NotFound"),

            AlbumNotFoundException e => new ErrorViewData(
                title: $"Album {e.AlbumId} not found",
                statusCode: HttpStatusCode.NotFound,
                viewName: "AlbumNotFound"),

            _ => new ErrorViewData(title: "Error")
        };
    }
}
