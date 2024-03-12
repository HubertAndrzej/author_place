using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.ViewModels.Albums;

public class AlbumSubscriptionViewModel
{
    public string? Title { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentType { get; set; }
    public Money? Paid { get; set; }
    public string? TransactionId { get; set; }
}
