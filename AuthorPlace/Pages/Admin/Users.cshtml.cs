using AuthorPlace.Models.Entities;
using AuthorPlace.Models.InputModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AuthorPlace.Pages.Admin
{
    [AllowAnonymous]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [BindProperty]
        public UserRoleInputModel? Input { get; set; }

        public IActionResult OnGet()
        {
            ViewData["Title"] = "Users management";
            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            if (!ModelState.IsValid)
            {
                return OnGet();
            }
            ApplicationUser user = await userManager.FindByEmailAsync(Input!.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(Input.Email), $"The email address '{Input.Email}' is not associated with any user.");
                return OnGet();
            }
            IList<Claim> claims = await userManager.GetClaimsAsync(user);
            Claim roleClaim = new(ClaimTypes.Role, Input.Role.ToString());
            if (claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
            {
                ModelState.AddModelError(nameof(Input.Role), $"The role '{Input.Role}' is already assigned to the user with email '{Input.Email}'.");
                return OnGet();
            }
            IdentityResult result = await userManager.AddClaimAsync(user, roleClaim);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"The assignment failed: {result.Errors.FirstOrDefault()?.Description}");
                return OnGet();
            }
            TempData["ConfirmationMessage"] = $"The role '{Input.Role}' has been assigned successfully to the user with email '{Input.Email}'";
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostRevokeAsync()
        {
            if (!ModelState.IsValid)
            {
                return OnGet();
            }
            ApplicationUser user = await userManager.FindByEmailAsync(Input!.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(Input.Email), $"The email address '{Input.Email}' is not associated with any user.");
                return OnGet();
            }
            IList<Claim> claims = await userManager.GetClaimsAsync(user);
            Claim roleClaim = new(ClaimTypes.Role, Input.Role.ToString());
            if (!claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
            {
                ModelState.AddModelError(nameof(Input.Role), $"The role '{Input.Role}' is already revoked from the user with email '{Input.Email}'.");
                return OnGet();
            }
            IdentityResult result = await userManager.RemoveClaimAsync(user, roleClaim);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"The assignment failed: {result.Errors.FirstOrDefault()?.Description}");
                return OnGet();
            }
            TempData["ConfirmationMessage"] = $"The role '{Input.Role}' has been revoked successfully from the user with email '{Input.Email}'";
            return RedirectToPage();
        }
    }
}
