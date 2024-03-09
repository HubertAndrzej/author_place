using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Pages
{
    public class ContactModel : PageModel
    {
        public AlbumDetailViewModel? Album { get; private set; }

        [Required(ErrorMessage = "The question could not be empty")]
        [Display(Name = "Text of the question")]
        [BindProperty]
        public string? Question { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, [FromServices] IAlbumService albumService)
        {
            try
            {
                Album = await albumService.GetAlbumAsync(id);
                return Page();
            }
            catch
            {
                return RedirectToAction("Index", "Albums");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id, [FromServices] IAlbumService albumService)
        {
            if (ModelState.IsValid)
            {
                await albumService.SendQuestionToAlbumAuthorAsync(id, Question);
                TempData["ConfirmationMessage"] = "Your question has been sent successfully";
                return RedirectToAction("Detail", "Albums", new { id });

            } else
            {
                return await OnGetAsync(id, albumService);
            }
        }

    }
}
