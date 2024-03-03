using FluentValidation;
using FluentValidation.Validators;

namespace AuthorPlace.Models.Validators.Albums;

public class RemotePropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>, IRemotePropertyValidator
{
    public string Url { get; }
    public IEnumerable<string> AdditionalFields { get; }
    public string ErrorText { get; }
    public override string Name => "RemotePropertyValidator";

    public RemotePropertyValidator(string url, string additionalFields, string errorText = "")
    {
        Url = url;
        AdditionalFields = (additionalFields ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        ErrorText = errorText;
    }

    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        return true;
    }
}
