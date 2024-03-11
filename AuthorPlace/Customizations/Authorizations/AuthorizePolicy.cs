using AuthorPlace.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AuthorPlace.Customizations.Authorizations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizePolicyAttribute : AuthorizeAttribute
{
    public AuthorizePolicyAttribute(Policy policy)
    {
        Policy = policy.ToString();
    }
}
