using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.Entities;

public class Album
{
    public Album(string title, string author, string authorId, string email)
    {
        ChangeTitle(title);
        ChangeAuthor(author, authorId);
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
        EnsureNotErased();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("The album must have a title");
        }
        Title = title;
    }

    public void ChangeAuthor(string author, string authorId)
    {
        EnsureNotErased();
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
        EnsureNotErased();
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
        EnsureNotErased();
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("The email must not be empty");
        }
        Email = email;
    }

    public void ChangeDescription(string description)
    {
        EnsureNotErased();
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
        EnsureNotErased();
        ImagePath = imagePath;
    }

    private void ChangeStatus(Status status)
    {
        EnsureNotErased();
        Status = status;
    }

    private void EnsureNotErased()
    {
        if (Status == Status.Erased)
        {
            throw new InvalidOperationException("The album is erased and cannot be modified");
        }
    }

    public void ChangeRating(double? rating)
    {
        if (rating == null)
        {
            return;
        }
        Rating = rating ?? 0;
    }

    public void Draft()
    {
        ChangeStatus(Status.Drafted);
    }

    public void Publish()
    {
        ChangeStatus(Status.Published);
    }

    public void Erase()
    {
        if (SubscribedUsers!.Any())
        {
            throw new AlbumDeletionException(Id);
        }
        ChangeStatus(Status.Erased);
    }
}
