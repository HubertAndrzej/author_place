using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using System.Data;

namespace AuthorPlace.Models.Extensions;

public static class DataRecordExtensions
{
    public static AlbumViewModel ToAlbumViewModel(this IDataRecord dataRecord)
    {
        AlbumViewModel albumViewModel = new()
        {
            Id = Convert.ToInt32(dataRecord["Id"]),
            Title = Convert.ToString(dataRecord["Title"]),
            ImagePath = Convert.ToString(dataRecord["ImagePath"]),
            Author = Convert.ToString(dataRecord["Author"]),
            Rating = Convert.ToDouble(dataRecord["Rating"]),
            FullPrice = new Money(
                    Enum.Parse<Currency>((string)dataRecord["FullPrice_Currency"]),
                    Convert.ToDecimal(dataRecord["FullPrice_Amount"])
                ),
            CurrentPrice = new Money(
                    Enum.Parse<Currency>((string)dataRecord["CurrentPrice_Currency"]),
                    Convert.ToDecimal(dataRecord["CurrentPrice_Amount"])
                )
        };
        return albumViewModel;
    }

    public static AlbumDetailViewModel ToAlbumDetailViewModel(this IDataRecord dataRecord)
    {
        AlbumDetailViewModel albumDetailViewModel = new()
        {
            Id = Convert.ToInt32(dataRecord["Id"]),
            Title = Convert.ToString(dataRecord["Title"]),
            Description = Convert.ToString(dataRecord["Description"]),
            ImagePath = Convert.ToString(dataRecord["ImagePath"]),
            Author = Convert.ToString(dataRecord["Author"]),
            Rating = Convert.ToDouble(dataRecord["Rating"]),
            FullPrice = new Money(
                    Enum.Parse<Currency>((string)dataRecord["FullPrice_Currency"]),
                    Convert.ToDecimal(dataRecord["FullPrice_Amount"])
                ),
            CurrentPrice = new Money(
                    Enum.Parse<Currency>((string)dataRecord["CurrentPrice_Currency"]),
                    Convert.ToDecimal(dataRecord["CurrentPrice_Amount"])
                ),
            Songs = new List<SongViewModel>()
        };
        return albumDetailViewModel;
    }

    public static SongViewModel ToSongViewModel(this IDataRecord dataRecord)
    {
        SongViewModel songViewModel = new()
        {
            Id = Convert.ToInt32(dataRecord["Id"]),
            Title = Convert.ToString(dataRecord["Title"]),
            Duration = TimeSpan.Parse((string)dataRecord["Duration"]),
        };
        return songViewModel;
    }
}
