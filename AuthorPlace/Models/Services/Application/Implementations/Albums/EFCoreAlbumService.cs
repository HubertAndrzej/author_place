using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;

namespace AuthorPlace.Models.Services.Application.Implementations.Albums;

public class EFCoreAlbumService : IAlbumService
{
    private readonly AuthorPlaceDbContext dbContext;
    private readonly IImagePersister imagePersister;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public EFCoreAlbumService(AuthorPlaceDbContext dbContext, IImagePersister imagePersister, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.dbContext = dbContext;
        this.imagePersister = imagePersister;
        this.albumsOptions = albumsOptions;
        logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel inputModel)
    {
        string orderby = inputModel.OrderBy == "CurrentPrice" ? "CurrentPrice.Amount" : inputModel.OrderBy;
        string direction = inputModel.Ascending ? "ASC" : "DESC";
        IQueryable<Album> baseQuery = dbContext.Albums!
            .OrderBy($"{orderby} {direction}");

        IQueryable<AlbumViewModel> queryLinq = baseQuery
            .AsNoTracking()
            .Where(album => album.Title!.Contains(inputModel.Search!))
            .Select(album => album.ToAlbumViewModel());
        List<AlbumViewModel> albums = await queryLinq
            .Skip(inputModel.Offset)
            .Take(inputModel.Limit)
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
            .Include(album => album.Songs)
            .Where(album => album.Id == id)
            .Select(album => album.ToAlbumDetailViewModel());
        AlbumDetailViewModel? viewModel = await queryLinq
            .FirstOrDefaultAsync();
        if (viewModel == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        return viewModel;
    }

    public async Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync()
    {
        AlbumListInputModel inputModel = new(search: "", page: 1, orderby: "Rating", ascending: false, limit: albumsOptions.CurrentValue.InHome, orderOptions: albumsOptions.CurrentValue.Order!);
        ListViewModel<AlbumViewModel> result = await GetAlbumsAsync(inputModel);
        return result.Results!;
    }

    public async Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync()
    {
        AlbumListInputModel inputModel = new(search: "", page: 1, orderby: "Id", ascending: false, limit: albumsOptions.CurrentValue.InHome, orderOptions: albumsOptions.CurrentValue.Order!);
        ListViewModel<AlbumViewModel> result = await GetAlbumsAsync(inputModel);
        return result.Results!;
    }

    public async Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel)
    {
        string title = inputModel.Title!;
        string author = "Hub Sobo";
        Album album = new(title, author);
        dbContext.Add(album);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            logger.LogWarning("Album with {title} by {author} already exists", title, author);
            throw new AlbumUniqueException(title, author, exception);
        }
        return album.ToAlbumDetailViewModel();
    }

    public async Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id)
    {
        IQueryable<AlbumUpdateInputModel> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Where(album => album.Id == id)
            .Select(album => album.ToAlbumUpdateInputModel());

        AlbumUpdateInputModel? inputModel = await queryLinq.FirstOrDefaultAsync();
        if (inputModel == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        return inputModel;
    }

    public async Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel)
    {
        string author = await GetAuthorAsync(inputModel.Id);
        Album? album = await dbContext.Albums!.FindAsync(inputModel.Id);
        if (album == null)
        {
            logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
            throw new AlbumNotFoundException(inputModel.Id);
        }
        album.ChangeTitle(inputModel.Title!);
        album.ChangePrices(inputModel.FullPrice!, inputModel.CurrentPrice!);
        album.ChangeDescription(inputModel.Description!);
        album.ChangeEmail(inputModel.Email!);
        dbContext.Entry(album).Property(album => album.RowVersion).OriginalValue = inputModel.RowVersion!;
        if (inputModel.Image != null)
        {
            try
            {
                string imagePath = await imagePersister.SaveAlbumImageAsync(inputModel.Id, inputModel.Image);
                album.ChangeImagePath(imagePath);
            }
            catch (Exception exception)
            {
                logger.LogWarning("The selected image could not be saved for album {inputModel.Id}", inputModel.Id);
                throw new AlbumImageInvalidException(inputModel.Id, exception);
            }
        }
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Update of album {inputModel.Id} failed", inputModel.Id);
            throw new OptimisticConcurrencyException();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
        return album.ToAlbumDetailViewModel();
    }

    public async Task RemoveAlbumAsync(AlbumDeleteInputModel inputModel)
    {
        Album? album = await dbContext.Albums!.FindAsync(inputModel.Id);
        if (album == null)
        {
            logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
            throw new AlbumNotFoundException(inputModel.Id);
        }
        album.ChangeStatus(Status.Erased);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsAlbumUniqueAsync(string title, string author, int id)
    {
        bool isAlbumUnique = await dbContext.Albums!.AnyAsync(album => EF.Functions.Like(album.Title!, title) && EF.Functions.Like(album.Author!, author) && album.Id != id);
        return !isAlbumUnique;
    }

    public async Task<string> GetAuthorAsync(int id)
    {
        IQueryable<string> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Where(album => album.Id == id)
            .Select(album => album.Author!);
        string? author = await queryLinq.FirstOrDefaultAsync();
        return author!;
    }
}
