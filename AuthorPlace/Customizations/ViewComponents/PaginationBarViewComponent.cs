using AuthorPlace.Models.ViewModels;
using AuthorPlace.Models.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Customizations.ViewComponents;

public class PaginationBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IPaginationInfo model)
    {
        return View(model);
    }
}
