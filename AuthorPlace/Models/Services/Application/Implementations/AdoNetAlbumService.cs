using AuthorPlace.Models.Exceptions;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using Microsoft.Extensions.Options;
using System.Data;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class AdoNetAlbumService : IAlbumService
{
    private readonly IDatabaseAccessor databaseAccessor;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public AdoNetAlbumService(IDatabaseAccessor databaseAccessor, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.databaseAccessor = databaseAccessor;
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
            orderby = "CurrentPrice_Amount";
        }
        string direction = ascending ? "ASC" : "DESC";
        FormattableString query = $"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Title LIKE '%{search}%' ORDER BY {(Sql) orderby} {(Sql) direction} LIMIT {limit} OFFSET {offset}";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        List<AlbumViewModel> albumList = new();
        foreach (DataRow albumRow in dataTable.Rows)
        {
            AlbumViewModel album = albumRow.ToAlbumViewModel();
            albumList.Add(album);
        }
        return albumList;
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        FormattableString query = $"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums WHERE Id = {id}; SELECT Id, Title, Description, Duration FROM Songs WHERE AlbumId = {id};";
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
}
