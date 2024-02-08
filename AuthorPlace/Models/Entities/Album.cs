using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.Entities;

public class Album
{
    public Album(string title, string author)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("The album must have a title");
        }
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("The album must have an author"); 
        }
        Title = title;
        Author = author;
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string? ImagePath { get; private set; }
    public string Author { get; private set; }
    public string? Email { get; private set; }
    public double Rating { get; private set; } = 0.0;
    public Money FullPrice { get; private set; } = new Money { Amount = 0.0m, Currency = Currency.EUR };
    public Money CurrentPrice { get; private set; } = new Money { Amount = 0.0m, Currency = Currency.EUR };
    public virtual ICollection<Song> Songs { get; private set; } = new List<Song>();

    public void ChangeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("The album must have a title");
        }
        Title = title;
    }

    public void ChangePrices(Money fullPrice, Money currentPrice)
    {
        if (fullPrice == null || currentPrice == null)
        {
            throw new ArgumentException("Prices can't be null");
        }
        if (fullPrice.Currency != currentPrice.Currency)
        {
            throw new ArgumentException("Currencies don't match");
        }
        if (fullPrice.Amount < currentPrice.Amount)
        {
            throw new ArgumentException("Full price can't be less than the current price");
        }
        FullPrice = fullPrice;
        CurrentPrice = currentPrice;
    }
}
