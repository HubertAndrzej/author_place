using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuthorPlace.Customizations.Authorizations;

public class MultiAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public MultiAuthorizationPolicyProvider(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> authorizationOptions) : base(authorizationOptions)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }
        string[] policyNames = policyName.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim()).ToArray();
        AuthorizationPolicyBuilder builder = new();
        builder.RequireAssertion(async (context) =>
        {
            IAuthorizationService authorizationService = httpContextAccessor.HttpContext!.RequestServices.GetService<IAuthorizationService>()!;
            foreach (string policyName in policyNames)
            {
                AuthorizationResult authorizationResult = await authorizationService.AuthorizeAsync(context.User, context.Resource, policyName);
                if (authorizationResult.Succeeded)
                {
                    return true;
                }
            }
            return false;
        });
        return builder.Build();
    }
}
