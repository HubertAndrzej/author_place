using AuthorPlace.Models.Enums;
using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.Entities;

public class Album
{
    public Album(string title, string author, string authorId, string email)
    {
        ChangeTitle(title);
        ChangeAuthor(author, authorId);
        ChangeStatus(Status.Drafted);
        Email = email;
        ImagePath = "/placeholder.jpg";
        FullPrice = new Money(Currency.EUR, 0);
        CurrentPrice = new Money(Currency.EUR, 0);
        Songs = new HashSet<Song>();
    }

    public int Id { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public string ImagePath { get; private set; }
    public string? AuthorId { get; private set; }
    public string? Author { get; private set; }
    public string? Email { get; private set; }
    public double Rating { get; private set; }
    public Money FullPrice { get; private set; }
    public Money CurrentPrice { get; private set; }
    public string? RowVersion { get; private set; }
    public Status Status { get; private set; }
    public virtual ICollection<Song> Songs { get; private set; } = new List<Song>();
    public virtual ApplicationUser? User { get; private set; }
    public virtual ICollection<ApplicationUser>? SubscribedUsers { get; private set; }

    public void ChangeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("The album must have a title");
        }
        Title = title;
    }

    public void ChangeAuthor(string author, string authorId)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("The album must have an author");
        }
        if (string.IsNullOrWhiteSpace(authorId))
        {
            throw new ArgumentException("The author must have an id");
        }
        Author = author;
        AuthorId = authorId;
    }

    public void ChangePrices(Money fullPrice, Money currentPrice)
    {
        if (fullPrice == null || currentPrice == null)
        {
            throw new ArgumentException("The prices must not be null");
        }
        if (fullPrice.Currency != currentPrice.Currency)
        {
            throw new ArgumentException("The currencies must match");
        }
        if (fullPrice.Amount < currentPrice.Amount)
        {
            throw new ArgumentException("The full price must be greater than the current price");
        }
        FullPrice = fullPrice;
        CurrentPrice = currentPrice;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("The email must not be empty");
        }
        Email = email;
    }

    public void ChangeDescription(string description)
    {
        if (description != null)
        {
            if (description.Length < 50)
            {
                throw new Exception("The description must have at least 50 characters");
            }
            else if (description.Length > 5000)
            {
                throw new Exception("The description must have at most 5000 characters");
            }
        }
        Description = description;
    }

    public void ChangeImagePath(string imagePath)
    {
        ImagePath = imagePath;
    }

    public void ChangeStatus(Status status)
    {
        Status = status;
    }
}
