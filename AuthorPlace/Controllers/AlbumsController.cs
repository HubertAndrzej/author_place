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
        return View(albums);
    }

    public IActionResult Detail(int id)
    {
        AlbumService albumService = new AlbumService();
        AlbumDetailViewModel album = albumService.GetAlbum(id);
        return View(album);
    }
}
