using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthorPlace.Customizations.Authorizations;

public class AlbumViewerRequirementHandler : AuthorizationHandler<AlbumViewerRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedAlbumService albumService;

    public AlbumViewerRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedAlbumService albumService)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.albumService = albumService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AlbumViewerRequirement requirement)
    {
        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int albumId;
        switch (httpContextAccessor.HttpContext!.Request.RouteValues["controller"]!.ToString()!.ToLowerInvariant())
        {
            case "albums":
                albumId = Convert.ToInt32(httpContextAccessor.HttpContext!.Request.RouteValues["id"]);
                break;
            default:
                context.Fail();
                return;
        }
        AlbumDetailViewModel viewModel = await albumService.GetAlbumAsync(albumId);
        if (viewModel.Status == Status.Published || viewModel.AuthorId == userId)
        {
            context.Succeed(requirement);
            return;
        }
        bool isSubscribed = await albumService.IsAlbumSubscribedAsync(albumId, userId);
        if (isSubscribed)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
