using AuthorPlace.Models.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels.Albums;

public class AlbumUpdateInputModel : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Title")]
    public string? Title { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Album Cover")]
    public string? ImagePath { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Full Price")]
    public Money? FullPrice { get; set; }

    [Display(Name = "Current Price")]
    public Money? CurrentPrice { get; set; }

    public IFormFile? Image { get; set; }

    public string? RowVersion { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullPrice!.Currency != CurrentPrice!.Currency)
        {
            yield return new ValidationResult("The full price must have the same currency of the current price", new[] { nameof(FullPrice) });
        }
        else if (FullPrice.Amount < CurrentPrice.Amount)
        {
            yield return new ValidationResult("The full price must be greater than or equal to the current price", new[] { nameof(FullPrice) });
        }
    }
}
