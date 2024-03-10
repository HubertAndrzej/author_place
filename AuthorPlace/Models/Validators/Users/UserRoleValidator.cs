using AuthorPlace.Models.InputModels.Users;
using FluentValidation;

namespace AuthorPlace.Models.Validators.Users;

public class UserRoleValidator : AbstractValidator<UserRoleInputModel>
{
    public UserRoleValidator()
    {
        RuleFor(model => model.Email)
        .NotEmpty().WithMessage("The email is mandatory")
        .EmailAddress().WithMessage("The email must have a valid format");

        RuleFor(model => model.Role)
        .NotEmpty().WithMessage("The role is mandatory");
    }
}
