using AuthorPlace.Controllers;
using AuthorPlace.Models.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels;

public class AlbumUpdateInputModel : IValidatableObject
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "The title is mandatory and cannot be made only of empty spaces")]
    [Remote(action: nameof(AlbumsController.IsAlbumUnique), controller: "Albums", ErrorMessage = "This title is already used by this author", AdditionalFields = "Id")]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    [MinLength(50, ErrorMessage = "The description must have at least {1} characters")]
    [MaxLength(5000, ErrorMessage = "The description must have at most {1} characters")]
    [Display(Name ="Description")]
    public string? Description { get; set; }

    [Display(Name = "Album Cover")]
    public string? ImagePath { get; set; }

    [Required(ErrorMessage = "The email is mandatory")]
    [EmailAddress(ErrorMessage = "The email must have a valid format")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "The full price is mandatory")]
    [Display(Name = "Full Price")]
    public Money? FullPrice { get; set; }

    [Required(ErrorMessage = "The current price is mandatory")]
    [Display(Name = "Current Price")]
    public Money? CurrentPrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullPrice!.Currency != CurrentPrice!.Currency)
        {
            yield return new ValidationResult("The full price must have the same currency of the current price", new[] { nameof(FullPrice), nameof(CurrentPrice) });
        }
        else if (FullPrice.Amount < CurrentPrice.Amount)
        {
            yield return new ValidationResult("The full price must be greater than the current price", new[] { nameof(FullPrice), nameof(CurrentPrice) });
        }
    }
}
