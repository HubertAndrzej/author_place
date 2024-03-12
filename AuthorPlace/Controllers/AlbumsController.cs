using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace AuthorPlace.Controllers;

public class AlbumsController : Controller
{
    private readonly IAlbumService albumService;

    public AlbumsController(ICachedAlbumService albumService)
    {
        this.albumService = albumService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(AlbumListInputModel inputModel)
    {
        ListViewModel<AlbumViewModel> albums = await albumService.GetAlbumsAsync(inputModel);
        ViewBag.Title = "Album Catalogue";
        AlbumListViewModel viewModel = new()
        {
            Albums = albums,
            Input = inputModel
        };
        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Detail(int id)
    {
        AlbumDetailViewModel viewModel = await albumService.GetAlbumAsync(id);
        ViewBag.Title = viewModel.Title;
        return View(viewModel);
    }

    public async Task<IActionResult> Pay(int id)
    {
        string paymentUrl = await albumService.GetPaymentUrlAsync(id);
        return Redirect(paymentUrl);
    }

    public async Task<IActionResult> Subscribe(int id, string token)
    {
        AlbumSubscribeInputModel inputModel = await albumService.CapturePaymentAsync(id, token);
        await albumService.SubscribeAlbumAsync(inputModel);
        TempData["ConfirmationMessage"] = "You have been subscribed successfully to this album";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = nameof(Policy.AlbumSubscriber))]
    public async Task<IActionResult> Vote(int albumId)
    {
        AlbumVoteInputModel inputModel = new()
        {
            Id = albumId,
            Vote = await albumService.GetAlbumVoteAsync(albumId) ?? 0
        };
        return View(inputModel);
    }

    [Authorize(Policy = nameof(Policy.AlbumSubscriber))]
    [HttpPost]
    public async Task<IActionResult> Vote(AlbumVoteInputModel inputModel)
    {
        await albumService.VoteAlbumAsync(inputModel);
        TempData["ConfirmationMessage"] = "Your vote have been submitted successfully";
        return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
    }

    [Authorize(Policy = nameof(Policy.AlbumSubscriber))]
    public async Task<IActionResult> Receipt(int albumId)
    {
        AlbumSubscriptionViewModel viewModel = await albumService.GetAlbumSubscriptionAsync(albumId);
        return new ViewAsPdf
        {
            Model = viewModel,
            ViewName = nameof(Receipt),
            PageMargins = new Margins { Top = 10, Left = 10, Bottom = 10, Right = 10 },
            PageSize = Size.A4,
            PageOrientation = Orientation.Portrait,
            FileName = $"{viewModel.Title} - purchase receipt.pdf"
        };
    }

    [Authorize(Roles = nameof(Role.Author))]
    public IActionResult New()
    {
        AlbumCreateInputModel inputModel = new();
        ViewBag.Title = "Create album";
        return View(inputModel);
    }

    [Authorize(Roles = nameof(Role.Author))]
    [HttpPost]
    public async Task<IActionResult> New(AlbumCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                AlbumDetailViewModel viewModel = await albumService.CreateAlbumAsync(inputModel);
                TempData["ConfirmationMessage"] = "Your album has been created successfully";
                return RedirectToAction(nameof(Edit), new { id = viewModel.Id });
            }
            catch (AlbumUniqueException)
            {
                ModelState.AddModelError(nameof(AlbumDetailViewModel.Title), "This title is already used by this author");
            }
        }
        ViewBag.Title = "Create album";
        return View(inputModel);
    }

    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    [Authorize(Roles = nameof(Role.Author))]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Update album";
        AlbumUpdateInputModel inputModel = await albumService.GetAlbumForEditingAsync(id);
        return View(inputModel);
    }

    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    [Authorize(Roles = nameof(Role.Author))]
    [HttpPost]
    public async Task<IActionResult> Edit(AlbumUpdateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                AlbumDetailViewModel viewModel = await albumService.UpdateAlbumAsync(inputModel);
                TempData["ConfirmationMessage"] = "Your album has been updated successfully";
                return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
            }
            catch (AlbumImageInvalidException)
            {
                ModelState.AddModelError(nameof(AlbumUpdateInputModel.Image), "The selected image cover is not valid");
            }
            catch (AlbumUniqueException)
            {
                ModelState.AddModelError(nameof(AlbumUpdateInputModel.Title), "This title is already used by this author");
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", $"The update failed because another user updated the values ​​in the meantime. Please refresh the page to get the updated values");
            }
        }
        ViewBag.Title = "Update album";
        return View(inputModel);
    }

    [Authorize(Policy = nameof(Policy.AlbumAuthor))]
    [Authorize(Roles = nameof(Role.Author))]
    [HttpPost]
    public async Task<IActionResult> Remove(AlbumDeleteInputModel inputModel)
    {
        await albumService.RemoveAlbumAsync(inputModel);
        TempData["ConfirmationMessage"] = "Your album has been deleted successfully";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = nameof(Role.Author))]
    public async Task<IActionResult> IsAlbumUnique(string title, string authorId, int id = 0)
    {
        bool result = await albumService.IsAlbumUniqueAsync(title, authorId, id);
        return Json(result);
    }
}
