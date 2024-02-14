﻿using AuthorPlace.Models.Services.Application.Interfaces;
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

    public async Task<IActionResult> Index(string? search = null, int page = 1, string orderby = "Rating", bool ascending = true)
    {
        List<AlbumViewModel> albums = await albumService.GetAlbumsAsync(search, page, orderby, ascending);
        ViewBag.Title = "Album Catalogue";
        return View(albums);
    }

    public async Task<IActionResult> Detail(int id)
    {
        AlbumDetailViewModel album = await albumService.GetAlbumAsync(id);
        ViewBag.Title = album.Title;
        return View(album);
    }
}
