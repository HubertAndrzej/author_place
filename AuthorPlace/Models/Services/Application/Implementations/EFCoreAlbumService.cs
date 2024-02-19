using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class EFCoreAlbumService : IAlbumService
{
    private readonly AuthorPlaceDbContext dbContext;
    private readonly ILogger logger;

    public EFCoreAlbumService(AuthorPlaceDbContext dbContext, ILoggerFactory loggerFactory)
    {
        this.dbContext = dbContext;
        this.logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model)
    {
        string orderby = model.OrderBy == "CurrentPrice" ? "CurrentPrice.Amount.ToString()" : model.OrderBy;
        string direction = model.Ascending ? "ASC" : "DESC";
        IQueryable<Album> baseQuery = dbContext.Albums!
            .OrderBy($"{orderby} {direction}");

        IQueryable<AlbumViewModel> queryLinq = baseQuery
            .AsNoTracking()
            .Where(album => album.Title.Contains(model.Search!))
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
            .Skip(model.Offset)
            .Take(model.Limit)
            .ToListAsync();
        int totalCount = await queryLinq.CountAsync();
        ListViewModel<AlbumViewModel> result = new()
        {
            Results = albums,
            TotalCount = totalCount
        };
        return result;
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
