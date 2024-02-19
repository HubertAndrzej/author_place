﻿using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class HomeController : Controller
{
    [ResponseCache(CacheProfileName = "Home")]
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
