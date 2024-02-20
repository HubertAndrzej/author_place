using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;
using System.Data;

namespace AuthorPlace.Models.Extensions;

public static class DataRecordExtensions
{
    public static AlbumViewModel ToAlbumViewModel(this IDataRecord dataRecord)
    {
        return new AlbumViewModel
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
    }

    public static AlbumDetailViewModel ToAlbumDetailViewModel(this IDataRecord dataRecord)
    {
        return new AlbumDetailViewModel
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
    }

    public static SongViewModel ToSongViewModel(this IDataRecord dataRecord)
    {
        return new SongViewModel
        {
            Id = Convert.ToInt32(dataRecord["Id"]),
            Title = Convert.ToString(dataRecord["Title"]),
            Duration = TimeSpan.Parse((string)dataRecord["Duration"]),
        };
    }
}
