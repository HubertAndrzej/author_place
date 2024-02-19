using AuthorPlace.Models.InputModels;
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

        if (Input.OrderBy == OrderBy)
        {
            string direction = Input.Ascending ? "up" : "down";
            string icon;
            if (OrderBy == "Title")
            {
                icon = Input.Ascending ? "a-z" : "z-a";
            }
            else
            {
                icon = Input.Ascending ? "1-9" : "9-1";
            }
            
            output.PostContent.SetHtmlContent($"<i class=\"fa-solid fa-arrow-{direction}-{icon}\"></i>");
        }
    }
}
