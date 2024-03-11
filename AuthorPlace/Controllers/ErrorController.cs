using AuthorPlace.Models.Services.Application.Interfaces.Errors;
using AuthorPlace.Models.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorPlace.Controllers;

public class ErrorController : Controller
{
    [AllowAnonymous]
    public IActionResult Index([FromServices] IErrorViewSelectorService errorViewSelectorService)
    {
        ErrorViewData errorViewData = errorViewSelectorService.GetErrorViewData(HttpContext);
        ViewBag.Title = errorViewData.Title;
        Response.StatusCode = (int) errorViewData.StatusCode;
        return View(errorViewData.ViewName);
    }
}
