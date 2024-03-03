using AuthorPlace.Models.InputModels.Songs;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Songs;

public class SongUpdateValidator : AbstractValidator<SongUpdateInputModel>
{
    public SongUpdateValidator()
    {
        RuleFor(model => model.Id)
        .NotEmpty();

        RuleFor(model => model.Title)
        .NotEmpty().WithMessage("The title is mandatory and cannot be made only of empty spaces");

        RuleFor(model => model.Description)
        .MinimumLength(50).WithMessage("The description must have at least 50 characters")
        .MaximumLength(5000).WithMessage("The description must have at most 100 characters");

        RuleFor(model => model.Duration)
        .NotEmpty().WithMessage("The duration is mandatory");
    }
}
