﻿using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AuthorPlace.Customizations.ModelBinders;

public class AlbumListInputModelBinder : IModelBinder
{
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;

    public AlbumListInputModelBinder(IOptionsMonitor<AlbumsOptions> albumsOptions)
    {
        this.albumsOptions = albumsOptions;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string search = bindingContext.ValueProvider.GetValue("Search").FirstValue!;
        int page = Convert.ToInt32(bindingContext.ValueProvider.GetValue("Page").FirstValue!);
        string orderby = bindingContext.ValueProvider.GetValue("OrderBy").FirstValue!;
        bool ascending = Convert.ToBoolean(bindingContext.ValueProvider.GetValue("Ascending").FirstValue!);
        AlbumsOptions options = albumsOptions.CurrentValue;
        AlbumListInputModel inputModel = new(search, page, orderby, ascending, options.PerPage, options.Order!);
        bindingContext.Result = ModelBindingResult.Success(inputModel);
        return Task.CompletedTask;
    }
}
