using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ViewModels;
using System.Data;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class AdoNetAlbumService : IAlbumService
{
    private readonly IDatabaseAccessor databaseAccessor;

    public AdoNetAlbumService(IDatabaseAccessor databaseAccessor)
    {
        this.databaseAccessor = databaseAccessor;
    }

    public async Task<List<AlbumViewModel>> GetAlbumsAsync()
    {
        FormattableString query = $"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Albums";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        List<AlbumViewModel> albumList = new List<AlbumViewModel>();
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
            throw new InvalidOperationException($"Did not return exactly 1 row for Album {id}");
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
