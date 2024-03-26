using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.Entities;

public class Subscription
{
    public Subscription()
    {

    }

    public Subscription(string userId, int albumId)
    {
        UserId = userId;
        AlbumId = albumId;
    }

    public string UserId { get; set; }
    public int AlbumId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentType { get; set; }
    public Money? Paid { get; set; }
    public string? TransactionId { get; set; }
    public int? Vote { get; set; }
    public virtual Album? Album { get; set; }
    public virtual ApplicationUser? User { get; set; }

}
