﻿using AuthorPlace.Controllers;
using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ViewModels.Albums;
using Ganss.Xss;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace AuthorPlace.Models.Services.Application.Implementations.Albums;

public class EFCoreAlbumService : IAlbumService
{
    private readonly AuthorPlaceDbContext dbContext;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IImagePersister imagePersister;
    private readonly IEmailClient emailClient;
    private readonly IPaymentGateway paymentGateway;
    private readonly LinkGenerator linkGenerator;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public EFCoreAlbumService(AuthorPlaceDbContext dbContext, IHttpContextAccessor httpContextAccessor, IImagePersister imagePersister, IEmailClient emailClient, IPaymentGateway paymentGateway, LinkGenerator linkGenerator, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.dbContext = dbContext;
        this.httpContextAccessor = httpContextAccessor;
        this.imagePersister = imagePersister;
        this.emailClient = emailClient;
        this.paymentGateway = paymentGateway;
        this.linkGenerator = linkGenerator;
        this.albumsOptions = albumsOptions;
        logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel inputModel)
    {
        string orderby = inputModel.OrderBy == "CurrentPrice" ? "CurrentPrice.Amount" : inputModel.OrderBy;
        string direction = inputModel.Ascending ? "ASC" : "DESC";
        IQueryable<Album> baseQuery = dbContext.Albums!
            .OrderBy($"{orderby} {direction}");

        IQueryable<AlbumViewModel> queryLinq = baseQuery
            .AsNoTracking()
            .Where(album => album.Title!.Contains(inputModel.Search!) && album.Status == Status.Published)
            .Select(album => album.ToAlbumViewModel());
        List<AlbumViewModel> albums = await queryLinq
            .Skip(inputModel.Offset)
            .Take(inputModel.Limit)
            .ToListAsync();
        int totalCount = await queryLinq.CountAsync();
        ListViewModel<AlbumViewModel> result = new()
        {
            Results = albums,
            TotalCount = totalCount
        };
        return result;
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        IQueryable<AlbumDetailViewModel> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Include(album => album.Songs)
            .Where(album => album.Id == id)
            .Select(album => album.ToAlbumDetailViewModel());
        AlbumDetailViewModel? viewModel = await queryLinq
            .FirstOrDefaultAsync();
        if (viewModel == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        return viewModel;
    }

    public async Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync()
    {
        AlbumListInputModel inputModel = new(search: "", page: 1, orderby: "Rating", ascending: false, limit: albumsOptions.CurrentValue.InHome, orderOptions: albumsOptions.CurrentValue.Order!);
        ListViewModel<AlbumViewModel> result = await GetAlbumsAsync(inputModel);
        return result.Results!;
    }

    public async Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync()
    {
        AlbumListInputModel inputModel = new(search: "", page: 1, orderby: "Id", ascending: false, limit: albumsOptions.CurrentValue.InHome, orderOptions: albumsOptions.CurrentValue.Order!);
        ListViewModel<AlbumViewModel> result = await GetAlbumsAsync(inputModel);
        return result.Results!;
    }

    public Task<List<AlbumViewModel>> GetAlbumsByAuthorAsync(string authorId)
    {
        return dbContext.Albums!
            .AsNoTracking()
            .Include(album => album.Songs)
            .Where(album => album.AuthorId == authorId)
            .Select(album => album.ToAlbumViewModel())
            .ToListAsync();
    }

    public Task<List<AlbumViewModel>> GetAlbumsBySubscriberAsync(string subscriberId)
    {
        return dbContext.Albums!
            .AsNoTracking()
            .Include(album => album.SubscribedUsers)
            .Where(album => album.SubscribedUsers!.Any(u => u.Id == subscriberId))
            .Select(album => album.ToAlbumViewModel())
            .ToListAsync();
    }

    public async Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel)
    {
        string title = inputModel.Title!;
        string author;
        string authorId;
        string email;
        try
        {
            author = httpContextAccessor.HttpContext!.User.FindFirst("FullName")!.Value;
            authorId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            email = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)!.Value;
        }
        catch (NullReferenceException)
        {
            throw new UserUnknownException();
        }
        Album album = new(title, author, authorId, email);
        dbContext.Add(album);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            logger.LogWarning("Album with {title} by {author} already exists", title, author);
            throw new AlbumUniqueException(title, author, exception);
        }
        return album.ToAlbumDetailViewModel();
    }

    public async Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id)
    {
        IQueryable<AlbumUpdateInputModel> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Where(album => album.Id == id)
            .Select(album => album.ToAlbumUpdateInputModel());

        AlbumUpdateInputModel? inputModel = await queryLinq.FirstOrDefaultAsync();
        if (inputModel == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        return inputModel;
    }

    public async Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel)
    {
        string author = await GetAuthorAsync(inputModel.Id);
        Album? album = await dbContext.Albums!.FindAsync(inputModel.Id);
        if (album == null)
        {
            logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
            throw new AlbumNotFoundException(inputModel.Id);
        }
        album.ChangeTitle(inputModel.Title!);
        album.ChangePrices(inputModel.FullPrice!, inputModel.CurrentPrice!);
        album.ChangeDescription(inputModel.Description!);
        album.ChangeEmail(inputModel.Email!);
        if (inputModel.IsPublished)
        {
            album.Publish();
        }
        else
        {
            album.Draft();
        }
        dbContext.Entry(album).Property(album => album.RowVersion).OriginalValue = inputModel.RowVersion!;
        if (inputModel.Image != null)
        {
            try
            {
                string imagePath = await imagePersister.SaveAlbumImageAsync(inputModel.Id, inputModel.Image);
                album.ChangeImagePath(imagePath);
            }
            catch (Exception exception)
            {
                logger.LogWarning("The selected image could not be saved for album {inputModel.Id}", inputModel.Id);
                throw new AlbumImageInvalidException(inputModel.Id, exception);
            }
        }
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Update of album {inputModel.Id} failed", inputModel.Id);
            throw new OptimisticConcurrencyException();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
        return album.ToAlbumDetailViewModel();
    }

    public async Task DeleteAlbumAsync(AlbumDeleteInputModel inputModel)
    {
        bool hasSubscribers = await dbContext.Subscriptions!.AnyAsync(subscription => subscription.AlbumId == inputModel.Id);
        if (hasSubscribers)
        {
            throw new AlbumDeletionException(inputModel.Id);
        }
        Album? album = await dbContext.Albums!.FindAsync(inputModel.Id);
        if (album == null)
        {
            logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
            throw new AlbumNotFoundException(inputModel.Id);
        }
        album.Erase();
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsAlbumUniqueAsync(string title, string authorId, int id)
    {
        bool isAlbumUnique = await dbContext.Albums!.AnyAsync(album => EF.Functions.Like(album.Title!, title) && EF.Functions.Like(album.AuthorId!, authorId) && album.Id != id);
        return !isAlbumUnique;
    }

    public async Task<string> GetAuthorAsync(int id)
    {
        IQueryable<string> queryLinq = dbContext.Albums!
            .AsNoTracking()
            .Where(album => album.Id == id)
            .Select(album => album.Author!);
        string? author = await queryLinq.FirstOrDefaultAsync();
        return author!;
    }

    public async Task SendQuestionToAlbumAuthorAsync(int id, string? question)
    {
        Album? album = await dbContext.Albums!.FindAsync(id);
        if (album == null)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        string albumTitle = album.Title!;
        string albumEmail = album.Email!;
        string userFullName;
        string userEmail;
        try
        {
            userFullName = httpContextAccessor.HttpContext!.User.FindFirst("FullName")!.Value;
            userEmail = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)!.Value;
        }
        catch (NullReferenceException)
        {
            throw new UserUnknownException();
        }
        HtmlSanitizer htmlSanitizer = new();
        htmlSanitizer.AllowedTags.Clear();
        question = htmlSanitizer.Sanitize(question!);
        string subject = $"Question for album '{albumTitle}'";
        string message = $"<p>'{userFullName}' (<a href=\"{userEmail}\">{userEmail}</a>) sent the following question:</p>\n<p>{question}</p>";
        try
        {
            await emailClient.SendEmailAsync(albumEmail, userEmail, subject, message);
        }
        catch
        {
            throw new SendException();
        }
    }

    public Task<string> GetAlbumAuthorIdAsync(int albumId)
    {
        return dbContext.Albums!
            .Where(album => album.Id == albumId)
            .Select(album => album.AuthorId)
            .FirstOrDefaultAsync()!;
    }

    public async Task SubscribeAlbumAsync(AlbumSubscribeInputModel inputModel)
    {
        AlbumDetailViewModel album = await GetAlbumAsync(inputModel.AlbumId);
        if (album.Status != Status.Published)
        {
            throw new AlbumSubscriptionException(inputModel.AlbumId);
        }
        Subscription subscription = new(inputModel.UserId!, inputModel.AlbumId)
        {
            PaymentDate = inputModel.PaymentDate,
            PaymentType = inputModel.PaymentType,
            Paid = inputModel.Paid,
            TransactionId = inputModel.TransactionId
        };

        dbContext.Subscriptions!.Add(subscription);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new AlbumSubscriptionException(inputModel.AlbumId);
        }
    }

    public Task<bool> IsAlbumSubscribedAsync(int albumId, string userId)
    {
        return dbContext.Subscriptions!.Where(subscription => subscription.AlbumId == albumId && subscription.UserId == userId).AnyAsync();
    }

    public async Task<string> GetPaymentUrlAsync(int albumId)
    {
        AlbumDetailViewModel viewModel = await GetAlbumAsync(albumId);
        string? returnUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext!, action: nameof(AlbumsController.Subscribe), controller: "Albums", values: new { id = albumId });
        string? cancelUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext!, action: nameof(AlbumsController.Detail), controller: "Albums", values: new { id = albumId });
        AlbumPayInputModel inputModel = new()
        {
            AlbumId = albumId,
            UserId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier),
            Description = viewModel.Title,
            Price = viewModel.CurrentPrice,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl
        };
        return await paymentGateway.GetPaymentUrlAsync(inputModel);
    }

    public Task<AlbumSubscribeInputModel> CapturePaymentAsync(int albumId, string token)
    {
        return paymentGateway.CapturePaymentAsync(token);
    }

    public async Task<AlbumSubscriptionViewModel> GetAlbumSubscriptionAsync(int albumId)
    {
        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        Subscription? subscription = await dbContext.Subscriptions!.Include(subscription => subscription.Album).SingleOrDefaultAsync(subscription => subscription.AlbumId == albumId && subscription.UserId == userId) ?? throw new AlbumSubscriptionNotFoundException(albumId);
        AlbumSubscriptionViewModel viewModel = subscription.ToAlbumSubscriptionViewModel();
        return viewModel;
    }

    public async Task<int?> GetAlbumVoteAsync(int albumId)
    {
        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        Subscription? subscription = await dbContext.Subscriptions!.SingleOrDefaultAsync(subscription => subscription.AlbumId == albumId && subscription.UserId == userId);
        return subscription == null ? throw new AlbumSubscriptionNotFoundException(albumId) : subscription.Vote;
    }

    public async Task VoteAlbumAsync(AlbumVoteInputModel inputModel)
    {
        if (inputModel.Vote < 1 || inputModel.Vote > 5)
        {
            throw new InvalidVoteException(inputModel.Vote);
        }
        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        Subscription? subscription = await dbContext.Subscriptions!.SingleOrDefaultAsync(subscription => subscription.AlbumId == inputModel.Id && subscription.UserId == userId) ?? throw new AlbumSubscriptionNotFoundException(inputModel.Id);
        subscription.Vote = inputModel.Vote;
        await dbContext.SaveChangesAsync();
    }
}
