using AuthorPlace.Customizations.ModelBinders;
using AuthorPlace.Models.Options;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Models.InputModels;

[ModelBinder(BinderType = typeof(AlbumListInputModelBinder))]
public class AlbumListInputModel
{
    public AlbumListInputModel(string search, int page, string orderby, bool ascending, AlbumsOptions albumsOptions)
    {
        AlbumsOrderOptions options = albumsOptions.Order!;
        if (!options.Allow!.Contains(orderby))
        {
            orderby = options.By!;
            ascending = options.Ascending;
        }

        Search = search ?? "";
        Page = Math.Max(1, page);
        OrderBy = orderby;
        Ascending = ascending;
        Limit = albumsOptions.PerPage;
        Offset = (Page - 1) * Limit;
    }

    public string? Search { get; }
    public int Page { get; }
    public string OrderBy { get; }
    public bool Ascending { get; }
    public int Limit { get; }
    public int Offset { get; }
}
