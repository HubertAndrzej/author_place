using AuthorPlace.Models.Exceptions;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using Microsoft.Extensions.Options;
using System.Data;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class AdoNetAsyncAlbumService : IAlbumService
{
    private readonly IDatabaseAccessor databaseAccessor;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public AdoNetAsyncAlbumService(IDatabaseAccessor databaseAccessor, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.databaseAccessor = databaseAccessor;
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
        int count = 0;
        FormattableString countQuery = $"SELECT COUNT(*) FROM Albums WHERE Title LIKE {"%" + model.Search + "%"};";
        IAsyncEnumerable<IDataRecord> countResults = databaseAccessor.QueryAsync(countQuery);
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
        FormattableString albumQuery = $"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Id = {id};";
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
        FormattableString songsQuery = $"SELECT Id, Title, Description, Duration FROM Songs WHERE AlbumId = {id};";
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
        FormattableString insertQuery = ($"INSERT INTO Albums (Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, '/placeholder.jpg', 'EUR', 0, 'EUR', 0);");
        await databaseAccessor.ExecuteAsync(insertQuery);
        FormattableString albumQuery = $"SELECT last_insert_rowid();";
        DataSet dataSet = await databaseAccessor.ExecuteAsync(albumQuery);
        int albumId = Convert.ToInt32(dataSet.Tables[0].Rows[0][0]);
        AlbumDetailViewModel? albumDetailViewModel = await GetAlbumAsync(albumId);
        return albumDetailViewModel;
    }
}
