using AuthorPlace.Models.InputModels.Albums;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Albums;

public class AlbumSubscribeValidator : AbstractValidator<AlbumSubscribeInputModel>
{
    public AlbumSubscribeValidator()
    {
        RuleFor(model => model.UserId)
        .NotEmpty();

        RuleFor(model => model.AlbumId)
        .NotEmpty();
    }
}
