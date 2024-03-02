using AuthorPlace.Models.InputModels.Songs;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Songs;

public class SongDeleteValidator : AbstractValidator<SongDeleteInputModel>
{
    public SongDeleteValidator()
    {
        RuleFor(model => model.Id)
        .NotEmpty();
    }
}
