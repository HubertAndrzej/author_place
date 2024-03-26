using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.ViewModels.Songs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class SongsController : Controller
{
    private readonly ICachedSongService songService;

    public SongsController(ICachedSongService songService)
    {
        this.songService = songService;
    }

    [Authorize(Policy = nameof(Policy.AlbumAuthor) + "," + nameof(Policy.AlbumSubscriber))]
    public IActionResult Detail(int id)
    {
        SongDetailViewModel viewModel = songService.GetSong(id);
        ViewBag.Title = viewModel.Title;
        return View(viewModel);
    }

    [Authorize(Roles = nameof(Role.Author))]
    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    public IActionResult New(int id)
    {
        SongCreateInputModel inputModel = new()
        {
            AlbumId = id
        };
        ViewBag.Title = "Create song";
        return View(inputModel);
    }

    [Authorize(Roles = nameof(Role.Author))]
    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    [HttpPost]
    public IActionResult New(SongCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            SongDetailViewModel song = songService.CreateSong(inputModel);
            TempData["ConfirmationMessage"] = "Your song has been created successfully";
            return RedirectToAction(nameof(Edit), new { id = song.Id });
        }
        ViewBag.Title = "Create song";
        return View(inputModel);
    }

    [Authorize(Roles = nameof(Role.Author))]
    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    public IActionResult Edit(int id)
    {
        ViewBag.Title = "Update song";
        SongUpdateInputModel inputModel = songService.GetSongForEditing(id);
        return View(inputModel);
    }

    [Authorize(Roles = nameof(Role.Author))]
    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    [HttpPost]
    public IActionResult Edit(SongUpdateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                SongDetailViewModel viewModel = songService.UpdateSong(inputModel);
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

    [Authorize(Roles = nameof(Role.Author))]
    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    [HttpPost]
    public IActionResult Remove(SongDeleteInputModel inputModel)
    {
        songService.RemoveSong(inputModel);
        TempData["ConfirmationMessage"] = "The song has been deleted successfully";
        return RedirectToAction(nameof(AlbumsController.Detail), "Albums", new { id = inputModel.AlbumId });
    }
}
