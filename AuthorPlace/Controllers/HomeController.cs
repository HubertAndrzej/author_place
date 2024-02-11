using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class HomeController : Controller
{
    [ResponseCache(CacheProfileName = "Home")]
    public IActionResult Index()
    {
        ViewBag.Title = "Home";
        return View();
    }
}
