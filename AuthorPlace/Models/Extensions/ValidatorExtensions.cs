using AuthorPlace.Models.Validators;
using FluentValidation;

namespace AuthorPlace.Models.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, TElement> Remote<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder, string url, string additionalFields, string errorText = "")
    {
        return ruleBuilder.SetValidator(new RemotePropertyValidator<T, TElement>(url, additionalFields, errorText));
    }
}
