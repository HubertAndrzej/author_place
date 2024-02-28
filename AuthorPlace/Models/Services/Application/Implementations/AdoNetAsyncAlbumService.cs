using AuthorPlace.Models.Exceptions;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Data;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class AdoNetAsyncAlbumService : IAlbumService
{
    private readonly IDatabaseAccessor databaseAccessor;
    private readonly IImagePersister imagePersister;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public AdoNetAsyncAlbumService(IDatabaseAccessor databaseAccessor, IImagePersister imagePersister, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.databaseAccessor = databaseAccessor;
        this.imagePersister = imagePersister;
        this.albumsOptions = albumsOptions;
        this.logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model)
    {
        string orderby = model.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : model.OrderBy;
        string direction = model.Ascending ? "ASC" : "DESC";
        FormattableString albumsQuery = $"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Title LIKE {"%" + model.Search + "%"} ORDER BY {(Sql) orderby} {(Sql) direction} LIMIT {model.Limit} OFFSET {model.Offset};";
        IAsyncEnumerable<IDataRecord> albumsResults = databaseAccessor.QueryAsync(albumsQuery);
        List<AlbumViewModel> albumList = new();
        await foreach (IDataRecord dataRecord in albumsResults)
        {
            AlbumViewModel albumViewModel = dataRecord.ToAlbumViewModel();
            albumList.Add(albumViewModel);
        }
        FormattableString countQuery = $"SELECT COUNT(*) FROM Albums WHERE Title LIKE {"%" + model.Search + "%"};";
        IAsyncEnumerable<IDataRecord> countResults = databaseAccessor.QueryAsync(countQuery);
        int count = 0;
        await foreach (IDataRecord dataRecord in countResults)
        {
            count = dataRecord.GetInt32(0);
            break;
        }
        ListViewModel<AlbumViewModel> result = new()
        {
            Results = albumList,
            TotalCount = count
        };
        return result;
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        FormattableString albumQuery = $"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Id={id};";
        IAsyncEnumerable<IDataRecord> albumResults = databaseAccessor.QueryAsync(albumQuery);
        AlbumDetailViewModel? albumDetailViewModel = null;
        await foreach (IDataRecord dataRecord in albumResults)
        {
            albumDetailViewModel = dataRecord.ToAlbumDetailViewModel();
            break;
        }
        if (albumDetailViewModel == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        FormattableString songsQuery = $"SELECT Id, Title, Description, Duration FROM Songs WHERE AlbumId={id};";
        IAsyncEnumerable<IDataRecord> songsResults = databaseAccessor.QueryAsync(songsQuery);
        await foreach (IDataRecord dataRecord in songsResults)
        {
            SongViewModel songViewModel = dataRecord.ToSongViewModel();
            albumDetailViewModel.Songs!.Add(songViewModel);
        }
        return albumDetailViewModel;
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
        try
        {
            FormattableString insertQuery = $"INSERT INTO Albums (Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, '/placeholder.jpg', 'EUR', 0, 'EUR', 0);";
            await databaseAccessor.ExecuteAsync(insertQuery);
            FormattableString albumQuery = $"SELECT last_insert_rowid();";
            IAsyncEnumerable<IDataRecord> idResults = databaseAccessor.QueryAsync(albumQuery);
            int albumId = 0;
            await foreach (IDataRecord dataRecord in idResults)
            {
                albumId = dataRecord.GetInt32(0);
                break;
            }
            AlbumDetailViewModel? albumDetailViewModel = await GetAlbumAsync(albumId);
            return albumDetailViewModel;
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            logger.LogWarning("Album with {title} by {author} not found", title, author);
            throw new AlbumUniqueException(title, author, exception);
        }
    }

    public async Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id)
    {
        FormattableString albumQuery = $"SELECT Id, Title, Description, ImagePath, Email, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Id={id};";
        IAsyncEnumerable<IDataRecord> albumResults = databaseAccessor.QueryAsync(albumQuery);
        AlbumUpdateInputModel? albumUpdateInputModel = null;
        await foreach (IDataRecord dataRecord in albumResults)
        {
            albumUpdateInputModel = dataRecord.ToAlbumUpdateInputModel();
            break;
        }
        if (albumUpdateInputModel == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        return albumUpdateInputModel;
    }

    public async Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel)
    {
        FormattableString countQuery = $"SELECT COUNT(*) FROM Albums WHERE Id={inputModel.Id}";
        IAsyncEnumerable<IDataRecord> countResults = databaseAccessor.QueryAsync(countQuery);
        int count = 0;
        await foreach (IDataRecord dataRecord in countResults)
        {
            count = dataRecord.GetInt32(0);
            break;
        }
        if (count == 0)
        {
            logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
            throw new AlbumNotFoundException(inputModel.Id);
        }
        string author = await GetAuthorAsync(inputModel.Id);
        try
        {
            FormattableString updateQuery = $"UPDATE Albums SET Title={inputModel.Title}, Description={inputModel.Description}, Email={inputModel.Email}, CurrentPrice_Currency={inputModel.CurrentPrice!.Currency}, CurrentPrice_Amount={inputModel.CurrentPrice.Amount}, FullPrice_Currency={inputModel.FullPrice!.Currency}, FullPrice_Amount={inputModel.FullPrice.Amount} WHERE Id={inputModel.Id}";
            await databaseAccessor.ExecuteAsync(updateQuery);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
        if (inputModel.Image != null)
        {
            try
            {
                string imagePath = await imagePersister.SaveAlbumImageAsync(inputModel.Id, inputModel.Image);
                FormattableString query = $"UPDATE Albums SET ImagePath={imagePath} WHERE Id={inputModel.Id};";
                await databaseAccessor.ExecuteAsync(query);
            }
            catch (Exception exception)
            {
                logger.LogWarning("The selected image could not be saved for album {inputModel.Id}", inputModel.Id);
                throw new AlbumImageInvalidException(inputModel.Id, exception);
            }
        }
        AlbumDetailViewModel? albumDetailViewModel = await GetAlbumAsync(inputModel.Id);
        return albumDetailViewModel;
    }

    public async Task<bool> IsAlbumUniqueAsync(string title, string author, int id)
    {
        FormattableString query = $"SELECT COUNT(*) FROM Albums WHERE Title LIKE {title} AND Author LIKE {author} AND Id<>{id};";
        IAsyncEnumerable<IDataRecord> countResults = databaseAccessor.QueryAsync(query);
        int count = 0;
        await foreach (IDataRecord dataRecord in countResults)
        {
            count = dataRecord.GetInt32(0);
            break;
        }
        bool isAlbumUnique = count == 0;
        return isAlbumUnique;
    }

    public async Task<string> GetAuthorAsync(int id)
    {
        FormattableString query = $"SELECT Author FROM Albums WHERE Id LIKE {id};";
        IAsyncEnumerable<IDataRecord> authorResult = databaseAccessor.QueryAsync(query);
        string author = "";
        await foreach (IDataRecord dataRecord in authorResult)
        {
            author = dataRecord.ToString()!;
            break;
        }
        return author;
    }
}
