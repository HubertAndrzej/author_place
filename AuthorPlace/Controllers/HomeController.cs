using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
