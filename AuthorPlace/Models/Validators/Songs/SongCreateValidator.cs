using AuthorPlace.Models.InputModels.Songs;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Songs;

public class SongCreateValidator : AbstractValidator<SongCreateInputModel>
{
    public SongCreateValidator()
    {
        RuleFor(model => model.AlbumId)
        .NotEmpty();

        RuleFor(model => model.Title)
        .NotEmpty().WithMessage("The title is mandatory and cannot be made only of empty spaces");
    }
}
