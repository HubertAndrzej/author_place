using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Albums;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Albums;

public class AlbumUpdateValidator : AbstractValidator<AlbumUpdateInputModel>
{
    public AlbumUpdateValidator()
    {
        RuleFor(model => model.Id)
        .NotEmpty();

        RuleFor(model => model.Title)
        .NotEmpty().WithMessage("The title is mandatory and cannot be made only of empty spaces")
        .Remote(url: "/Albums/IsAlbumUnique", additionalFields: "Id", errorText: "This title is already used by this author");

        RuleFor(model => model.Description)
        .MinimumLength(50).WithMessage("The description must have at least 50 characters")
        .MaximumLength(5000).WithMessage("The description must have at most 100 characters");

        RuleFor(model => model.Email)
        .NotEmpty().WithMessage("The email is mandatory")
        .EmailAddress().WithMessage("The email must have a valid format");

        RuleFor(model => model.FullPrice)
        .NotEmpty().WithMessage("The full price is mandatory");

        RuleFor(model => model.CurrentPrice)
        .NotEmpty().WithMessage("The current price is mandatory");
    }
}
