using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using System.Data;

namespace AuthorPlace.Models.Extensions;

public static class DataRowExtensions
{
    public static AlbumViewModel ToAlbumViewModel(this DataRow albumRow)
    {
        AlbumViewModel albumViewModel = new()
        {
            Id = Convert.ToInt32(albumRow["Id"]),
            Title = Convert.ToString(albumRow["Title"]),
            ImagePath = Convert.ToString(albumRow["ImagePath"]),
            Author = Convert.ToString(albumRow["Author"]),
            Rating = Convert.ToDouble(albumRow["Rating"]),
            FullPrice = new Money(
                    Enum.Parse<Currency>((string)albumRow["FullPrice_Currency"]),
                    Convert.ToDecimal(albumRow["FullPrice_Amount"])
                ),
            CurrentPrice = new Money(
                    Enum.Parse<Currency>((string)albumRow["CurrentPrice_Currency"]),
                    Convert.ToDecimal(albumRow["CurrentPrice_Amount"])
                )
        };
        return albumViewModel;
    }

    public static AlbumDetailViewModel ToAlbumDetailViewModel(this DataRow albumRow)
    {
        AlbumDetailViewModel albumDetailViewModel = new()
        {
            Id = Convert.ToInt32(albumRow["Id"]),
            Title = Convert.ToString(albumRow["Title"]),
            Description = Convert.ToString(albumRow["Description"]),
            ImagePath = Convert.ToString(albumRow["ImagePath"]),
            Author = Convert.ToString(albumRow["Author"]),
            Rating = Convert.ToDouble(albumRow["Rating"]),
            FullPrice = new Money(
                    Enum.Parse<Currency>((string)albumRow["FullPrice_Currency"]),
                    Convert.ToDecimal(albumRow["FullPrice_Amount"])
                ),
            CurrentPrice = new Money(
                    Enum.Parse<Currency>((string)albumRow["CurrentPrice_Currency"]),
                    Convert.ToDecimal(albumRow["CurrentPrice_Amount"])
                ),
            Songs = new List<SongViewModel>()
        };
        return albumDetailViewModel;
    }

    public static SongViewModel ToSongViewModel(this DataRow songRow)
    {
        SongViewModel songViewModel = new()
        {
            Id = Convert.ToInt32(songRow["Id"]),
            Title = Convert.ToString(songRow["Title"]),
            Duration = TimeSpan.Parse((string)songRow["Duration"]),
        };
        return songViewModel;
    }
}
