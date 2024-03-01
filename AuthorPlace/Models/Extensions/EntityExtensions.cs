using AuthorPlace.Models.Entities;
using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.ViewModels;

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
            Songs = album.Songs.Select(song => song.ToSongViewModel()).ToList()
        };
    }

    public static SongViewModel ToSongViewModel(this Song song)
    {
        return new SongViewModel
        {
            Id = song.Id,
            Title = song.Title,
            Duration = song.Duration,
            Description = song.Description
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
}
