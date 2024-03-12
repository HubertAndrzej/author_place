using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.InputModels.Albums;

public class AlbumSubscribeInputModel
{
    public string? UserId { get; set; }
    public int AlbumId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentType { get; set; }
    public Money? Paid { get; set; }
    public string? TransactionId { get; set; }
}
