using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class AlbumsController : Controller
{
    private readonly IAlbumService albumService;

    public AlbumsController(IAlbumService albumService)
    {
        this.albumService = albumService;
    }

    public IActionResult Index()
    {
        List<AlbumViewModel> albums = albumService.GetAlbums();
        ViewBag.Title = "Album Catalogue";
        return View(albums);
    }

    public IActionResult Detail(int id)
    {
        AlbumDetailViewModel album = albumService.GetAlbum(id);
        ViewBag.Title = album.Title;
        return View(album);
    }
}
