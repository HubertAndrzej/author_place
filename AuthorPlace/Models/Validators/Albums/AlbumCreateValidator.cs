using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Albums;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Albums;

public class AlbumCreateValidator : AbstractValidator<AlbumCreateInputModel>
{
    public AlbumCreateValidator()
    {
        RuleFor(model => model.Title)
        .NotEmpty().WithMessage("The title is mandatory and cannot be made only of empty spaces")
        .Remote(url: "/Albums/IsAlbumUnique", additionalFields: "Id", errorText: "This title is already used by this author");
    }
}
