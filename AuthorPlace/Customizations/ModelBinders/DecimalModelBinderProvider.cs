﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthorPlace.Customizations.ModelBinders;

public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(decimal))
        {
            return new DecimalModelBinder();
        }
        return null;
    }
}
