using AuthorPlace.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AuthorPlace.Customizations.Claims;

public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public CustomClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        ClaimsIdentity claimsIdentity = await base.GenerateClaimsAsync(user);
        claimsIdentity.AddClaim(new Claim("FullName", user.FullName!));
        return claimsIdentity;
    }
}