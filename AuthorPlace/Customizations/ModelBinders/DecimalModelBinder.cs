﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace AuthorPlace.Customizations.ModelBinders;

public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue!;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
        {
            bindingContext.Result = ModelBindingResult.Success(decimalValue);
        }
        return Task.CompletedTask;
    }
}
