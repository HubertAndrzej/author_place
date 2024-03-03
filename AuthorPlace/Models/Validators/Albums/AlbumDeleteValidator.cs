using AuthorPlace.Models.InputModels.Albums;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Albums;

public class AlbumDeleteValidator : AbstractValidator<AlbumDeleteInputModel>
{
    public AlbumDeleteValidator()
    {
        RuleFor(model => model.Id)
        .NotEmpty();
    }
}
