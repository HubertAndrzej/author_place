using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class AlbumsController : Controller
{
    public IActionResult Index()
    {
        return Content("Controller: Albums\nAction: Index");
    }

    public IActionResult Detail(int id)
    {
        return Content($"Controller: Albums\nAction: Detail\nId: {id}");
    }
}
