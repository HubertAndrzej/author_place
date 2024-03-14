using AuthorPlace.Models.Services.Worker.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthorPlace.Controllers;

public class UserDataController : Controller
{
    private readonly IUserDataService userDataService;

    public UserDataController(IUserDataService userDataService)
    {
        this.userDataService = userDataService;
    }

    [Authorize]
    public IActionResult Download(Guid id)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string zipFilePath = userDataService.GetUserDataZipFileLocation(userId, id);
        _ = new FileInfo(zipFilePath);
        if (System.IO.File.Exists(zipFilePath))
        {
            return PhysicalFile(zipFilePath, "application/zip", "Albums.zip");
        }
        else
        {
            return NotFound();
        }
    }
}
