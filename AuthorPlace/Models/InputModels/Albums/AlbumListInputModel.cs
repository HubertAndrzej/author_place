using AuthorPlace.Customizations.ModelBinders;
using AuthorPlace.Models.Options;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Models.InputModels.Albums;

[ModelBinder(BinderType = typeof(AlbumListInputModelBinder))]
public class AlbumListInputModel
{
    public AlbumListInputModel(string search, int page, string orderby, bool ascending, int limit, AlbumsOrderOptions orderOptions)
    {
        if (!orderOptions.Allow!.Contains(orderby))
        {
            orderby = orderOptions.By!;
            ascending = orderOptions.Ascending;
        }

        Search = search ?? "";
        Page = Math.Max(1, page);
        OrderBy = orderby;
        Ascending = ascending;
        Limit = Math.Max(1, limit);
        Offset = (Page - 1) * Limit;
    }

    public string? Search { get; }
    public int Page { get; }
    public string OrderBy { get; }
    public bool Ascending { get; }
    public int Limit { get; }
    public int Offset { get; }
}
