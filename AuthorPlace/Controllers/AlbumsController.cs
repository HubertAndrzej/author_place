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
        List<AlbumViewModel> albums = await albumService.GetAlbumsAsync(input);
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
}
