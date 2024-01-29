using AuthorPlace.Models.Services.Application;
using AuthorPlace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class AlbumsController : Controller
{
    public IActionResult Index()
    {
        AlbumService albumService = new AlbumService();
        List<AlbumViewModel> albums = albumService.GetAlbums();
        ViewBag.Title = "Album Catalogue";
        return View(albums);
    }

    public IActionResult Detail(int id)
    {
        AlbumService albumService = new AlbumService();
        AlbumDetailViewModel album = albumService.GetAlbum(id);
        ViewBag.Title = album.Title;
        return View(album);
    }
}
