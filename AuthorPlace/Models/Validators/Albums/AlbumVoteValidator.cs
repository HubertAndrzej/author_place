using AuthorPlace.Models.InputModels.Albums;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Albums;

public class AlbumVoteValidator : AbstractValidator<AlbumVoteInputModel>
{
	public AlbumVoteValidator()
	{
        RuleFor(model => model.Id)
        .NotEmpty();

        RuleFor(model => model.Vote)
        .NotEmpty();
    }
}
