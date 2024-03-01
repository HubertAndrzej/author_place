using AuthorPlace.Models.Enums;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using System.Data;

namespace AuthorPlace.Models.Extensions;

public static class DataRowExtensions
{
    public static AlbumViewModel ToAlbumViewModel(this DataRow albumRow)
    {
        return new AlbumViewModel
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
    }

    public static AlbumDetailViewModel ToAlbumDetailViewModel(this DataRow albumRow)
    {
        return new AlbumDetailViewModel
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
    }

    public static SongViewModel ToSongViewModel(this DataRow songRow)
    {
        return new SongViewModel
        {
            Id = Convert.ToInt32(songRow["Id"]),
            Title = Convert.ToString(songRow["Title"]),
            Duration = TimeSpan.Parse((string)songRow["Duration"]),
        };
    }

    public static AlbumUpdateInputModel ToAlbumUpdateInputModel(this DataRow dataRow)
    {
        return new AlbumUpdateInputModel
        {
            Id = Convert.ToInt32(dataRow["Id"]),
            Title = Convert.ToString(dataRow["Title"]),
            Description = Convert.ToString(dataRow["Description"]),
            Email = Convert.ToString(dataRow["Email"]),
            ImagePath = Convert.ToString(dataRow["ImagePath"]),
            FullPrice = new Money(
                Enum.Parse<Currency>((string)dataRow["FullPrice_Currency"]),
                Convert.ToDecimal(dataRow["FullPrice_Amount"])
            ),
            CurrentPrice = new Money(
                Enum.Parse<Currency>((string)dataRow["CurrentPrice_Currency"]),
                Convert.ToDecimal(dataRow["CurrentPrice_Amount"])
            ),
            RowVersion = Convert.ToString(dataRow["RowVersion"])
        };
    }
}
