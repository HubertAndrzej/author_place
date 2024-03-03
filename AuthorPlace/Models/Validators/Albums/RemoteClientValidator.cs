using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AuthorPlace.Models.Validators.Albums;

public class RemoteClientValidator : ClientValidatorBase
{
    public RemoteClientValidator(IValidationRule rule, IRuleComponent component) : base(rule, component)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        IRemotePropertyValidator validator = (IRemotePropertyValidator)Validator;
        string additionalFields = string.Join(',', (new[] { context.ModelMetadata.PropertyName }).Union(validator.AdditionalFields).Select(field => $"*.{field}"));
        MergeAttribute(context.Attributes, "data-val-remote-url", validator.Url);
        MergeAttribute(context.Attributes, "data-val-remote-additionalfields", additionalFields);
        MergeAttribute(context.Attributes, "data-val-remote", validator.ErrorText);
    }
}
