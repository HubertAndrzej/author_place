using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class ErrorController : Controller
{
    public IActionResult Index([FromServices] IErrorViewSelectorService errorViewSelectorService)
    {
        ErrorViewData errorViewData = errorViewSelectorService.GetErrorViewData(HttpContext);
        ViewBag.Title = errorViewData.Title;
        Response.StatusCode = (int) errorViewData.StatusCode;
        return View(errorViewData.ViewName);
    }
}
