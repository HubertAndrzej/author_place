using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class AlbumsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Detail(int id)
    {
        return View();
    }
}
