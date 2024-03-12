using AuthorPlace.Models.Entities;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.ViewModels.Albums;
using AuthorPlace.Models.ViewModels.Songs;

namespace AuthorPlace.Models.Extensions;

public static class EntityExtensions
{
    public static AlbumViewModel ToAlbumViewModel(this Album album)
    {
        return new AlbumViewModel
        {
            Id = album.Id,
            Title = album.Title,
            ImagePath = album.ImagePath,
            Author = album.Author,
            Rating = album.Rating,
            CurrentPrice = album.CurrentPrice,
            FullPrice = album.FullPrice
        };
    }

    public static AlbumDetailViewModel ToAlbumDetailViewModel(this Album album)
    {
        return new AlbumDetailViewModel
        {
            Id = album.Id,
            Title = album.Title,
            Description = album.Description,
            Author = album.Author,
            ImagePath = album.ImagePath,
            Rating = album.Rating,
            CurrentPrice = album.CurrentPrice,
            FullPrice = album.FullPrice,
            Songs = album.Songs.OrderBy(song => song.Id).Select(song => song.ToSongViewModel()).ToList()
        };
    }

    public static AlbumUpdateInputModel ToAlbumUpdateInputModel(this Album album)
    {
        return new AlbumUpdateInputModel
        {
            Id = album.Id,
            Title = album.Title,
            Description = album.Description,
            Email = album.Email,
            ImagePath = album.ImagePath,
            CurrentPrice = album.CurrentPrice,
            FullPrice = album.FullPrice,
            RowVersion = album.RowVersion
        };
    }

    public static AlbumSubscriptionViewModel ToAlbumSubscriptionViewModel(this Subscription subscription)
    {
        return new AlbumSubscriptionViewModel
        {
            Title = subscription.Album!.Title,
            Paid = subscription.Paid,
            PaymentDate = subscription.PaymentDate,
            PaymentType = subscription.PaymentType,
            TransactionId = subscription.TransactionId
        };
    }

    public static SongViewModel ToSongViewModel(this Song song)
    {
        return new SongViewModel
        {
            Id = song.Id,
            Title = song.Title,
            Duration = song.Duration
        };
    }

    public static SongDetailViewModel ToSongDetailViewModel(this Song song)
    {
        return new SongDetailViewModel
        {
            Id = song.Id,
            AlbumId = song.AlbumId,
            Title = song.Title,
            Duration = song.Duration,
            Description = song.Description
        };
    }

    public static SongUpdateInputModel ToSongUpdateInputModel(this Song song)
    {
        return new SongUpdateInputModel
        {
            Id = song.Id,
            AlbumId = song.AlbumId,
            Title = song.Title,
            Description = song.Description,
            Duration = song.Duration,
            RowVersion = song.RowVersion
        };
    }
}
