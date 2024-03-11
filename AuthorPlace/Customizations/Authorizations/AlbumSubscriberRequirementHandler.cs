using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthorPlace.Customizations.Authorizations;

public class AlbumSubscriberRequirementHandler : AuthorizationHandler<AlbumSubscriberRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedAlbumService albumService;
    private readonly ISongService songService;

    public AlbumSubscriberRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedAlbumService albumService, ISongService songService)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.albumService = albumService;
        this.songService = songService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AlbumSubscriberRequirement requirement)
    {
        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int albumId;
        if (context.Resource is int i)
        {
            albumId = i;
        }
        else
        {
            int id = Convert.ToInt32(httpContextAccessor.HttpContext!.Request.RouteValues["id"]);
            if (id == 0)
            {
                context.Fail();
                return;
            }
            switch (httpContextAccessor.HttpContext.Request.RouteValues["controller"]!.ToString()!.ToLowerInvariant())
            {
                case "songs":
                    albumId = (await songService.GetSongAsync(id)).AlbumId;
                    break;
                case "albums":
                    albumId = id;
                    break;
                default:
                    context.Fail();
                    return;
            }
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
