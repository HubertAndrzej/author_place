using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Exceptions.Infrastructure;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels.Albums;
using AuthorPlace.Models.ViewModels.Songs;
using Microsoft.Extensions.Options;
using System.Data;

namespace AuthorPlace.Models.Services.Application.Implementations.Albums;

public class AdoNetAlbumService : IAlbumService
{
    private readonly IDatabaseAccessor databaseAccessor;
    private readonly IImagePersister imagePersister;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public AdoNetAlbumService(IDatabaseAccessor databaseAccessor, IImagePersister imagePersister, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.databaseAccessor = databaseAccessor;
        this.imagePersister = imagePersister;
        this.albumsOptions = albumsOptions;
        logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model)
    {
        string orderby = model.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : model.OrderBy;
        string direction = model.Ascending ? "ASC" : "DESC";
        FormattableString query = $"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Title LIKE {"%" + model.Search + "%"} ORDER BY {(Sql)orderby} {(Sql)direction} LIMIT {model.Limit} OFFSET {model.Offset}; SELECT COUNT(*) FROM Albums WHERE Title LIKE {"%" + model.Search + "%"};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        List<AlbumViewModel> albumList = new();
        foreach (DataRow albumRow in dataTable.Rows)
        {
            AlbumViewModel album = albumRow.ToAlbumViewModel();
            albumList.Add(album);
        }
        ListViewModel<AlbumViewModel> result = new()
        {
            Results = albumList,
            TotalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
        };
        return result;
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        FormattableString query = $"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Id={id}; SELECT Id, Title, Description, Duration FROM Songs WHERE AlbumId={id};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable albumTable = dataSet.Tables[0];
        if (albumTable.Rows.Count != 1)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        DataRow albumRow = albumTable.Rows[0];
        AlbumDetailViewModel albumDetailViewModel = albumRow.ToAlbumDetailViewModel();
        DataTable songDataTable = dataSet.Tables[1];
        foreach (DataRow songRow in songDataTable.Rows)
        {
            SongViewModel songViewModel = songRow.ToSongViewModel();
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
            await databaseAccessor.CommandAsync(insertQuery);
            FormattableString albumQuery = $"SELECT last_insert_rowid();";
            int albumId = await databaseAccessor.ScalarAsync<int>(albumQuery);
            AlbumDetailViewModel? albumDetailViewModel = await GetAlbumAsync(albumId);
            return albumDetailViewModel;
        }
        catch (ConstraintViolationException exception)
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
    }

    public async Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id)
    {
        FormattableString query = $"SELECT Id, Title, Description, ImagePath, Email, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, RowVersion FROM Albums WHERE Id={id};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable albumTable = dataSet.Tables[0];
        if (albumTable.Rows.Count != 1)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        DataRow albumRow = albumTable.Rows[0];
        AlbumUpdateInputModel albumUpdateInputModel = albumRow.ToAlbumUpdateInputModel();
        return albumUpdateInputModel;
    }

    public async Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel)
    {
        string author = await GetAuthorAsync(inputModel.Id);
        try
        {
            string? imagePath = null;
            if (inputModel.Image != null)
            {
                imagePath = await imagePersister.SaveAlbumImageAsync(inputModel.Id, inputModel.Image);
            }
            FormattableString updateQuery = $"UPDATE Albums SET Title={inputModel.Title}, Description={inputModel.Description}, ImagePath=COALESCE({imagePath}, ImagePath), Email={inputModel.Email}, CurrentPrice_Currency={inputModel.CurrentPrice!.Currency}, CurrentPrice_Amount={inputModel.CurrentPrice.Amount}, FullPrice_Currency={inputModel.FullPrice!.Currency}, FullPrice_Amount={inputModel.FullPrice.Amount} WHERE Id={inputModel.Id} AND RowVersion={inputModel.RowVersion}";
            int affectedRows = await databaseAccessor.CommandAsync(updateQuery);
            if (affectedRows == 0)
            {
                FormattableString countQuery = $"SELECT COUNT(*) FROM Albums WHERE Id={inputModel.Id};";
                bool albumExists = await databaseAccessor.ScalarAsync<bool>(countQuery);
                if (albumExists)
                {
                    logger.LogWarning("Update of album {inputModel.Id} failed", inputModel.Id);
                    throw new OptimisticConcurrencyException();
                }
                else
                {
                    logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
                    throw new AlbumNotFoundException(inputModel.Id);
                }
            }
        }
        catch (ConstraintViolationException exception)
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
        catch (ImagePersistenceException exception)
        {
            logger.LogWarning("The selected image could not be saved for album {inputModel.Id}", inputModel.Id);
            throw new AlbumImageInvalidException(inputModel.Id, exception);
        }
        AlbumDetailViewModel? albumDetailViewModel = await GetAlbumAsync(inputModel.Id);
        return albumDetailViewModel;
    }

    public async Task<bool> IsAlbumUniqueAsync(string title, string author, int id)
    {
        FormattableString query = $"SELECT COUNT(*) FROM Albums WHERE Title LIKE {title} AND Author LIKE {author} AND Id<>{id};";
        bool isTitleAvailable = await databaseAccessor.ScalarAsync<bool>(query);
        return !isTitleAvailable;
    }

    public async Task<string> GetAuthorAsync(int id)
    {
        FormattableString query = $"SELECT Author FROM Albums WHERE Id LIKE {id};";
        string author = await databaseAccessor.ScalarAsync<string>(query);
        return author;
    }
}
