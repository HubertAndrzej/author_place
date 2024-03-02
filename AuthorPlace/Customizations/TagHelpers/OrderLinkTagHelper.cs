using AuthorPlace.Models.InputModels.Albums;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AuthorPlace.Customizations.TagHelpers;

public class OrderLinkTagHelper : AnchorTagHelper
{
    public string? OrderBy { get; set; }
    public AlbumListInputModel? Input { get; set; }

    public OrderLinkTagHelper(IHtmlGenerator generator) : base(generator)
    {
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.Attributes.Add("class", "link-info link-underline-opacity-0 link-underline-opacity-25-hover bold");
        RouteValues["search"] = Input!.Search;
        RouteValues["orderby"] = OrderBy;
        RouteValues["ascending"] = (Input.OrderBy == OrderBy ? !Input.Ascending : Input.Ascending).ToString().ToLowerInvariant();

        base.Process(context, output);

        string html = (Input.OrderBy, Input.Ascending) switch
        {
            ("Title", true) => "<i class=\"fa-solid fa-arrow-up-a-z\"></i>",
            ("Title", false) => "<i class=\"fa-solid fa-arrow-down-z-a\"></i>",
            ("Rating", true) => "<i class=\"fa-solid fa-arrow-up-1-9\"></i>",
            ("Rating", false) => "<i class=\"fa-solid fa-arrow-down-9-1\"></i>",
            ("CurrentPrice", true) => "<i class=\"fa-solid fa-arrow-up-1-9\"></i>",
            ("CurrentPrice", false) => "<i class=\"fa-solid fa-arrow-down-9-1\"></i>",
            _ => ""
        };

        if (Input.OrderBy == OrderBy)
        {
            output.PostContent.SetHtmlContent(html);
        }
    }
}
