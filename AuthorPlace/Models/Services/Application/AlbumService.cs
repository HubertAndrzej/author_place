using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application;

public class AlbumService
{
    public List<AlbumViewModel> GetAlbums()
    {
        List<AlbumViewModel> albumsList = new List<AlbumViewModel>();
        var random = new Random();
        for (int i = 1; i <= 20; i++)
        {
            decimal price = Convert.ToDecimal(random.NextDouble() * 10 + 10);
            var album = new AlbumViewModel
            {
                Id = i,
                Title = $"Album {i}",
                ImagePath = "placeholder.jpg",
                Author = "Author's Name",
                Rating = random.NextDouble() * 5,
                FullPrice = new Money(Currency.EUR, random.NextDouble() > 0.5 ? price : price - 1),
                CurrentPrice = new Money(Currency.EUR, price)
            };
            albumsList.Add(album);
        }
        return albumsList;
    }
}
