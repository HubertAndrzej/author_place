using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class AlbumService : IAlbumService
{
    public List<AlbumViewModel> GetAlbums()
    {
        List<AlbumViewModel> albumsList = new List<AlbumViewModel>();
        Random random = new Random();
        for (int i = 1; i <= 20; i++)
        {
            decimal price = Convert.ToDecimal(random.NextDouble() * 10 + 10);
            AlbumViewModel album = new AlbumViewModel
            {
                Id = i,
                Title = $"Album {i}",
                ImagePath = "placeholder.jpg",
                Author = "Author's Name",
                Rating = random.NextDouble() * 5,
                FullPrice = new Money(Currency.EUR, random.NextDouble() > 0.5 ? price : price + 1),
                CurrentPrice = new Money(Currency.EUR, price)
            };
            albumsList.Add(album);
        }
        return albumsList;
    }

    public AlbumDetailViewModel GetAlbum(int id)
    {
        Random random = new Random();
        decimal price = Convert.ToDecimal(random.NextDouble() * 10 + 10);
        AlbumDetailViewModel album = new AlbumDetailViewModel
        {
            Id = id,
            Title = $"Album {id}",
            ImagePath = "placeholder.jpg",
            Author = "Author's Name",
            Rating = random.Next(10, 50) / 10,
            FullPrice = new Money(Currency.EUR, random.NextDouble() > 0.5 ? price : price + 1),
            CurrentPrice = new Money(Currency.EUR, price),
            Description = $"Description {id}",
            Songs = new List<SongViewModel>()
        };
        for (int i = 1; i <= 10; i++)
        {
            SongViewModel song = new SongViewModel
            {
                Title = $"Song {i}",
                Duration = TimeSpan.FromSeconds(random.Next(30, 150))
            };
            album.Songs.Add(song);
        }
        return album;
    }
}
