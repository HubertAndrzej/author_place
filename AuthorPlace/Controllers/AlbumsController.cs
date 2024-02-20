using AuthorPlace.Models.Entities;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
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

    public IActionResult Create()
    {
        AlbumCreateInputModel inputModel = new();
        ViewBag.Title = "New album";
        return View(inputModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AlbumCreateInputModel inputModel)
    {
        AlbumDetailViewModel album = await albumService.CreateAlbumAsync(inputModel);
        return RedirectToAction(nameof(Index));
    }
}
