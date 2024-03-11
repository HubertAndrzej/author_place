using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.ViewModels.Songs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

[Authorize(Roles = nameof(Role.Author))]
public class SongsController : Controller
{
    private readonly ICachedSongService songService;

    public SongsController(ICachedSongService songService)
    {
        this.songService = songService;
    }

    public async Task<IActionResult> Detail(int id)
    {
        SongDetailViewModel viewModel = await songService.GetSongAsync(id);
        ViewBag.Title = viewModel.Title;
        return View(viewModel);
    }

    public IActionResult New(int id)
    {
        SongCreateInputModel inputModel = new()
        {
            AlbumId = id
        };
        ViewBag.Title = "Create song";
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> New(SongCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            SongDetailViewModel song = await songService.CreateSongAsync(inputModel);
            TempData["ConfirmationMessage"] = "Your song has been created successfully";
            return RedirectToAction(nameof(Edit), new { id = song.Id });
        }
        ViewBag.Title = "Create song";
        return View(inputModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Update song";
        SongUpdateInputModel inputModel = await songService.GetSongForEditingAsync(id);
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(SongUpdateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                SongDetailViewModel viewModel = await songService.UpdateSongAsync(inputModel);
                TempData["ConfirmationMessage"] = "Your song has been updated successfully";
                return RedirectToAction(nameof(Detail), new { id = viewModel.Id });
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", $"The update failed because another user updated the values ​​in the meantime. Please refresh the page to get the updated values");
            }
        }
        ViewBag.Title = "Update song";
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> Remove(SongDeleteInputModel inputModel)
    {
        await songService.RemoveSongAsync(inputModel);
        TempData["ConfirmationMessage"] = "The song has been deleted successfully";
        return RedirectToAction(nameof(AlbumsController.Detail), "Albums", new { id = inputModel.AlbumId });
    }
}
