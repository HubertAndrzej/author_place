﻿using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class EFCoreAlbumService : IAlbumService
{
    private readonly AuthorPlaceDbContext dbContext;

    public EFCoreAlbumService(AuthorPlaceDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<AlbumViewModel>> GetAlbumsAsync()
    {
        IQueryable<AlbumViewModel> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Select(album => new AlbumViewModel
            {
                Id = album.Id,
                Title = album.Title,
                ImagePath = album.ImagePath,
                Author = album.Author,
                Rating = album.Rating,
                FullPrice = album.FullPrice,
                CurrentPrice = album.CurrentPrice
            });
        List<AlbumViewModel> albums = await queryLinq
            .ToListAsync();
        return albums;
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        IQueryable<AlbumDetailViewModel> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Where(album => album.Id == id)
            .Select(album => new AlbumDetailViewModel
            {
                Id = album.Id,
                Title = album.Title,
                ImagePath = album.ImagePath,
                Author = album.Author,
                Rating = album.Rating,
                FullPrice = album.FullPrice,
                CurrentPrice = album.CurrentPrice,
                Description = album.Description,
                Songs = album.Songs
                .Select(song => new SongViewModel
                {
                    Id = song.Id,
                    Title = song.Title,
                    Description = song.Description,
                    Duration = song.Duration
                })
                .ToList()
            });
        AlbumDetailViewModel album = await queryLinq
            .SingleAsync();
        return album;
    }
}
