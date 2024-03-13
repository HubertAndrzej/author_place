using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Services.Worker.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AuthorPlace.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = nameof(Role.Author))]
    public class DownloadUserDataModel : PageModel
    {
        private readonly IUserDataService _userDataService;

        public DownloadUserDataModel(IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        public IActionResult OnPost()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _userDataService.EnqueueUserDataDownload(userId);
            return Page();
        }
    }
}
