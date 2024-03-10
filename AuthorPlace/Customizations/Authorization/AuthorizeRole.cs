using AuthorPlace.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AuthorPlace.Customizations.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params Role[] roles)
    {
        Roles = string.Join(",", roles.Select(role => role.ToString()));
    }
}
