using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class EFCoreAlbumService : IAlbumService
{
    private readonly AuthorPlaceDbContext dbContext;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public EFCoreAlbumService(AuthorPlaceDbContext dbContext, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.dbContext = dbContext;
        this.albumsOptions = albumsOptions;
        this.logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<List<AlbumViewModel>> GetAlbumsAsync(string? search, int page, string orderby, bool ascending)
    {
        search ??= "";
        page = Math.Max(1, page);
        int limit = albumsOptions.CurrentValue.PerPage;
        int offset = (page - 1) * limit;
        AlbumsOrderOptions options = albumsOptions.CurrentValue.Order!;
        if (!options.Allow!.Contains(orderby))
        {
            orderby = options.By!;
            ascending = options.Ascending;
        }
        if (orderby == "CurrentPrice")
        {
            orderby = "CurrentPrice.Amount.ToString()";
        }
        string direction = ascending ? "ASC" : "DESC";
        IQueryable<Album> baseQuery = dbContext.Albums!.OrderBy($"{orderby} {direction}");

        IQueryable<AlbumViewModel> queryLinq = baseQuery
            .AsNoTracking()
            .Skip(offset)
            .Take(limit)
            .Where(album => album.Title.Contains(search))
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
        AlbumDetailViewModel? album = await queryLinq
            .FirstOrDefaultAsync();
        if (album == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        return album;
    }
}
