using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    public IActionResult Create()
    {
        AlbumCreateInputModel inputModel = new();
        ViewBag.Title = "New album";
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AlbumCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                AlbumDetailViewModel album = await albumService.CreateAlbumAsync(inputModel);
                return RedirectToAction(nameof(Index));
            }
            catch (AlbumUniqueException)
            {
                ModelState.AddModelError(nameof(AlbumDetailViewModel.Title), "This title is already used by this author");
            }
        }
        ViewBag.Title = "New album";
        return View(inputModel);
    }

    public async Task<IActionResult> IsAlbumUnique(string title, string author)
    {
        bool result = await albumService.IsAlbumUnique(title, author);
        return Json(result);
    }
}
