﻿using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class AlbumsController : Controller
{
    private readonly IAlbumService albumService;

    public AlbumsController(ICachedAlbumService albumService)
    {
        this.albumService = albumService;
    }

    public async Task<IActionResult> Index(AlbumListInputModel input)
    {
        ListViewModel<AlbumViewModel> albums = await albumService.GetAlbumsAsync(input);
        ViewBag.Title = "Album Catalogue";
        AlbumListViewModel viewModel = new()
        {
            Albums = albums,
            Input = input
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Detail(int id)
    {
        AlbumDetailViewModel album = await albumService.GetAlbumAsync(id);
        ViewBag.Title = album.Title;
        return View(album);
    }

    public IActionResult New()
    {
        AlbumCreateInputModel inputModel = new();
        ViewBag.Title = "Create album";
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> New(AlbumCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                AlbumDetailViewModel album = await albumService.CreateAlbumAsync(inputModel);
                TempData["ConfirmationMessage"] = "Your album has been created successfully";
                return RedirectToAction(nameof(Edit), new { id = album.Id });
            }
            catch (AlbumUniqueException)
            {
                ModelState.AddModelError(nameof(AlbumDetailViewModel.Title), "This title is already used by this author");
            }
        }
        ViewBag.Title = "Create album";
        return View(inputModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Update album";
        AlbumUpdateInputModel inputModel = await albumService.GetAlbumForEditingAsync(id);
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AlbumUpdateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                AlbumDetailViewModel album = await albumService.UpdateAlbumAsync(inputModel);
                TempData["ConfirmationMessage"] = "Your album has been updated successfully";
                return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
            }
            catch (AlbumImageInvalidException)
            {
                ModelState.AddModelError(nameof(AlbumUpdateInputModel.Image), "The selected image cover is not valid");
            }
            catch (AlbumUniqueException)
            {
                ModelState.AddModelError(nameof(AlbumUpdateInputModel.Title), "This title is already used by this author");
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", $"The update failed because another user updated the values ​​in the meantime. Please refresh the page to get the updated values");
            }
        }
        ViewBag.Title = "Update album";
        return View(inputModel);
    }

    public async Task<IActionResult> IsAlbumUnique(string title, string author, int id = 0)
    {
        bool result = await albumService.IsAlbumUniqueAsync(title, author, id);
        return Json(result);
    }
}
