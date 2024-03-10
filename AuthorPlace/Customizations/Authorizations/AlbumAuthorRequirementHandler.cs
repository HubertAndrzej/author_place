using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthorPlace.Customizations.Authorizations
{
    public class AlbumAuthorRequirementHandler : AuthorizationHandler<AlbumAuthorRequirement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICachedAlbumService albumService;

        public AlbumAuthorRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedAlbumService albumService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.albumService = albumService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AlbumAuthorRequirement requirement)
        {
            string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int albumId = Convert.ToInt32(httpContextAccessor.HttpContext!.Request.RouteValues["id"]);
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
}
