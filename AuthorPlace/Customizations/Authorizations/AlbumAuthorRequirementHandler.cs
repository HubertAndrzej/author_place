using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthorPlace.Customizations.Authorizations;

public class AlbumAuthorRequirementHandler : AuthorizationHandler<AlbumAuthorRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedAlbumService albumService;
    private readonly ISongService songService;

    public AlbumAuthorRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedAlbumService albumService, ISongService songService)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.albumService = albumService;
        this.songService = songService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AlbumAuthorRequirement requirement)
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
                    if (httpContextAccessor.HttpContext!.Request.RouteValues["action"]!.ToString()!.ToLowerInvariant() == "new")
                    {
                        albumId = id;
                    }
                    else
                    {
                        albumId = (await songService.GetSongAsync(id)).AlbumId;
                    }
                    break;
                case "albums":
                    albumId = id;
                    break;
                default:
                    context.Fail();
                    return;
            }
        }
        string authorId = await albumService.GetAlbumAuthorIdAsync(albumId);
        if (userId == authorId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
