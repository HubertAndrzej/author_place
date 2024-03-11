using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using AuthorPlace.Models.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    [ResponseCache(CacheProfileName = "Home", Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index([FromServices] ICachedAlbumService albumService)
    {
        ViewBag.Title = "Welcome on AuthorPlace!";
        List<AlbumViewModel> bestRatingAlbums = await albumService.GetBestRatingAlbumsAsync();
        List<AlbumViewModel> mostRecentAlbums = await albumService.GetMostRecentAlbumsAsync();

        HomeViewModel viewModel = new()
        {
            BestRatingAlbums = bestRatingAlbums,
            MostRecentAlbums = mostRecentAlbums
        };
        return View(viewModel);
    }
}
