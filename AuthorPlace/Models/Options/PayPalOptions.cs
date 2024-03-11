namespace AuthorPlace.Models.Options;

public class PayPalOptions
{
    public string? ClientId { get; set; }
    public string? Secret { get; set; }
    public bool IsSandbox { get; set; }
    public string? BrandName { get; set; }
}
