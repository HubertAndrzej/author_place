﻿using AuthorPlace.Controllers;
using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Exceptions.Infrastructure;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ValueObjects;
using AuthorPlace.Models.ViewModels.Albums;
using AuthorPlace.Models.ViewModels.Songs;
using Ganss.Xss;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Claims;

namespace AuthorPlace.Models.Services.Application.Implementations.Albums;

public class AdoNetAlbumService : IAlbumService
{
    private readonly IDatabaseAccessor databaseAccessor;
    private readonly IImagePersister imagePersister;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IEmailClient emailClient;
    private readonly IPaymentGateway paymentGateway;
    private readonly LinkGenerator linkGenerator;
    private readonly IOptionsMonitor<AlbumsOptions> albumsOptions;
    private readonly ILogger logger;

    public AdoNetAlbumService(IDatabaseAccessor databaseAccessor, IImagePersister imagePersister, IHttpContextAccessor httpContextAccessor, IEmailClient emailClient, IPaymentGateway paymentGateway, LinkGenerator linkGenerator, IOptionsMonitor<AlbumsOptions> albumsOptions, ILoggerFactory loggerFactory)
    {
        this.databaseAccessor = databaseAccessor;
        this.imagePersister = imagePersister;
        this.httpContextAccessor = httpContextAccessor;
        this.emailClient = emailClient;
        this.paymentGateway = paymentGateway;
        this.linkGenerator = linkGenerator;
        this.albumsOptions = albumsOptions;
        logger = loggerFactory.CreateLogger("Albums");
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel inputModel)
    {
        string orderby = inputModel.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : inputModel.OrderBy;
        string direction = inputModel.Ascending ? "ASC" : "DESC";
        FormattableString selectQuery = $"SELECT Id, Title, ImagePath, Author, AuthorId, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, Status FROM Albums WHERE Title LIKE {"%" + inputModel.Search + "%"} AND Status={nameof(Status.Published)} ORDER BY {(Sql)orderby} {(Sql)direction} LIMIT {inputModel.Limit} OFFSET {inputModel.Offset};";
        DataSet dataSet = await databaseAccessor.QueryAsync(selectQuery);
        DataTable dataTable = dataSet.Tables[0];
        List<AlbumViewModel> albumList = new();
        foreach (DataRow albumRow in dataTable.Rows)
        {
            AlbumViewModel album = albumRow.ToAlbumViewModel();
            albumList.Add(album);
        }
        FormattableString countQuery = $"SELECT COUNT(*) FROM Albums WHERE Title LIKE {"%" + inputModel.Search + "%"} AND Status={nameof(Status.Published)};";
        int count = await databaseAccessor.ScalarAsync<int>(countQuery);
        ListViewModel<AlbumViewModel> result = new()
        {
            Results = albumList,
            TotalCount = count
        };
        return result;
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        FormattableString albumQuery = $"SELECT Id, Title, Description, ImagePath, Author, AuthorId, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, Status FROM Albums WHERE Id={id} AND Status<>{nameof(Status.Erased)};";
        DataSet dataSet = await databaseAccessor.QueryAsync(albumQuery);
        DataTable albumTable = dataSet.Tables[0];
        if (albumTable.Rows.Count != 1)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        DataRow albumRow = albumTable.Rows[0];
        AlbumDetailViewModel viewModel = albumRow.ToAlbumDetailViewModel();
        FormattableString songQuery = $"SELECT Id, Title, Description, Duration FROM Songs WHERE AlbumId={id} ORDER BY Id;";
        dataSet = await databaseAccessor.QueryAsync(songQuery);
        DataTable songTable = dataSet.Tables[0];
        foreach (DataRow songRow in songTable.Rows)
        {
            SongViewModel songViewModel = songRow.ToSongViewModel();
            viewModel.Songs!.Add(songViewModel);
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

    public async Task<List<AlbumViewModel>> GetAlbumsByAuthorAsync(string authorId)
    {
        FormattableString query = $@"SELECT Id, Title, ImagePath, Author, AuthorId, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, Status FROM Albums WHERE AuthorId={authorId} AND Status<>{nameof(Status.Erased)};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        List<AlbumViewModel> albums = new();
        foreach (DataRow albumRow in dataTable.Rows)
        {
            AlbumViewModel album = albumRow.ToAlbumViewModel();
            albums.Add(album);
        }
        return albums;
    }

    public async Task<List<AlbumViewModel>> GetAlbumsBySubscriberAsync(string subscriberId)
    {
        FormattableString query = $"SELECT Id, Title, ImagePath, Author, AuthorId, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, Status FROM Albums INNER JOIN Subscriptions ON Albums.Id=Subscriptions.AlbumId WHERE Status<>{nameof(Status.Erased)} AND Subscriptions.UserId={subscriberId};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        List<AlbumViewModel> albums = new();
        foreach (DataRow albumRow in dataTable.Rows)
        {
            AlbumViewModel album = albumRow.ToAlbumViewModel();
            albums.Add(album);
        }
        return albums;
    }

    public async Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel)
    {
        string title = inputModel.Title!;
        string author;
        string authorId;

        try
        {
            author = httpContextAccessor.HttpContext!.User.FindFirst("FullName")!.Value;
            authorId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }
        catch (NullReferenceException)
        {
            throw new UserUnknownException();
        }
        try
        {
            FormattableString insertQuery = $"INSERT INTO Albums (Title, Author, AuthorId, ImagePath, Rating, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount, Status) VALUES ({title}, {author}, {authorId}, '/placeholder.jpg', 0, 'EUR', 0, 'EUR', 0, {nameof(Status.Drafted)});";
            await databaseAccessor.CommandAsync(insertQuery);
            FormattableString albumQuery = $"SELECT last_insert_rowid();";
            int albumId = await databaseAccessor.ScalarAsync<int>(albumQuery);
            AlbumDetailViewModel? viewModel = await GetAlbumAsync(albumId);
            return viewModel;
        }
        catch (ConstraintViolationException exception)
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
    }

    public async Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id)
    {
        FormattableString query = $"SELECT Id, Title, Description, ImagePath, Email, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, Status, RowVersion FROM Albums WHERE Id={id} AND Status<>{nameof(Status.Erased)};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable albumTable = dataSet.Tables[0];
        if (albumTable.Rows.Count != 1)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        DataRow albumRow = albumTable.Rows[0];
        AlbumUpdateInputModel inputModel = albumRow.ToAlbumUpdateInputModel();
        return inputModel;
    }

    public async Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel)
    {
        string author = await GetAuthorAsync(inputModel.Id);
        try
        {
            string? imagePath = null;
            if (inputModel.Image != null)
            {
                imagePath = await imagePersister.SaveAlbumImageAsync(inputModel.Id, inputModel.Image);
            }
            FormattableString updateQuery = $"UPDATE Albums SET Title={inputModel.Title}, Description={inputModel.Description}, ImagePath=COALESCE({imagePath}, ImagePath), Email={inputModel.Email}, CurrentPrice_Currency={inputModel.CurrentPrice!.Currency}, CurrentPrice_Amount={inputModel.CurrentPrice.Amount}, FullPrice_Currency={inputModel.FullPrice!.Currency}, FullPrice_Amount={inputModel.FullPrice.Amount}, Status=({(inputModel.IsPublished ? Status.Published : Status.Drafted)}, Status) WHERE Id={inputModel.Id} AND RowVersion={inputModel.RowVersion} AND Status<>{nameof(Status.Erased)}";
            int affectedRows = await databaseAccessor.CommandAsync(updateQuery);
            if (affectedRows == 0)
            {
                FormattableString countQuery = $"SELECT COUNT(*) FROM Albums WHERE Id={inputModel.Id};";
                bool albumExists = await databaseAccessor.ScalarAsync<bool>(countQuery);
                if (albumExists)
                {
                    logger.LogWarning("Update of album {inputModel.Id} failed", inputModel.Id);
                    throw new OptimisticConcurrencyException();
                }
                else
                {
                    logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
                    throw new AlbumNotFoundException(inputModel.Id);
                }
            }
        }
        catch (ConstraintViolationException exception)
        {
            logger.LogWarning("Album with {inputModel.Title} by {author} already exists", inputModel.Title, author);
            throw new AlbumUniqueException(inputModel.Title!, author, exception);
        }
        catch (ImagePersistenceException exception)
        {
            logger.LogWarning("The selected image could not be saved for album {inputModel.Id}", inputModel.Id);
            throw new AlbumImageInvalidException(inputModel.Id, exception);
        }
        AlbumDetailViewModel? viewModel = await GetAlbumAsync(inputModel.Id);
        return viewModel;
    }

    public async Task DeleteAlbumAsync(AlbumDeleteInputModel inputModel)
    {
        FormattableString countQuery = $"SELECT COUNT(*) FROM Subscriptions WHERE AlbumId={inputModel.Id};";
        int count = await databaseAccessor.ScalarAsync<int>(countQuery);
        if (count > 0)
        {
            throw new AlbumDeletionException(inputModel.Id);
        }
        FormattableString deleteQuery = $"UPDATE Albums SET Status={nameof(Status.Erased)} WHERE Id={inputModel.Id} AND Status<>{nameof(Status.Erased)}";
        int affectedRows = await databaseAccessor.CommandAsync(deleteQuery);
        if (affectedRows == 0)
        {
            logger.LogWarning("Album {inputModel.Id} not found", inputModel.Id);
            throw new AlbumNotFoundException(inputModel.Id);
        }
    }

    public async Task<bool> IsAlbumUniqueAsync(string title, string authorId, int id)
    {
        FormattableString query = $"SELECT COUNT(*) FROM Albums WHERE Title LIKE {title} AND AuthorId LIKE {authorId} AND Id<>{id};";
        bool isTitleAvailable = await databaseAccessor.ScalarAsync<bool>(query);
        return !isTitleAvailable;
    }

    public async Task<string> GetAuthorAsync(int id)
    {
        FormattableString query = $"SELECT Author FROM Albums WHERE Id LIKE {id};";
        string author = await databaseAccessor.ScalarAsync<string>(query);
        return author;
    }

    public async Task SendQuestionToAlbumAuthorAsync(int id, string? question)
    {
        FormattableString query = $"SELECT Title, Email FROM Albums WHERE Id={id};";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        if (dataTable.Rows.Count == 0)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new AlbumNotFoundException(id);
        }
        string? albumTitle = Convert.ToString(dataTable.Rows[0]["Title"]);
        string? albumEmail = Convert.ToString(dataTable.Rows[0]["Email"]);
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
            await emailClient.SendEmailAsync(albumEmail!, userEmail, subject, message);
        }
        catch
        {
            throw new SendException();
        }
    }

    public Task<string> GetAlbumAuthorIdAsync(int albumId)
    {
        FormattableString query = $"SELECT AuthorId FROM Albums WHERE Id={albumId};";
        return databaseAccessor.ScalarAsync<string>(query);
    }

    public async Task SubscribeAlbumAsync(AlbumSubscribeInputModel inputModel)
    {
        AlbumDetailViewModel album = await GetAlbumAsync(inputModel.AlbumId);
        if (album.Status != Status.Published)
        {
            throw new AlbumSubscriptionException(inputModel.AlbumId);
        }
        try
        {
            FormattableString query = $"INSERT INTO Subscriptions (UserId, AlbumId, PaymentDate, PaymentType, Paid_Currency, Paid_Amount, TransactionId) VALUES ({inputModel.UserId}, {inputModel.AlbumId}, {inputModel.PaymentDate}, {inputModel.PaymentType}, {inputModel.Paid!.Currency}, {inputModel.Paid.Amount}, {inputModel.TransactionId});";
            await databaseAccessor.CommandAsync(query);
        }
        catch (ConstraintViolationException)
        {
            throw new AlbumSubscriptionException(inputModel.AlbumId);
        }
    }

    public Task<bool> IsAlbumSubscribedAsync(int albumId, string userId)
    {
        FormattableString query = $"SELECT COUNT(*) FROM Subscriptions WHERE AlbumId={albumId} AND UserId={userId};";
        return databaseAccessor.ScalarAsync<bool>(query);
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
        FormattableString query = $"SELECT Title, PaymentDate, PaymentType, Paid_Amount, Paid_Currency, TransactionId FROM Subscriptions INNER JOIN Albums ON Subscriptions.AlbumId = Albums.Id WHERE Subscriptions.AlbumId={albumId} AND Subscriptions.UserId={userId}";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable dataTable = dataSet.Tables[0];
        if (dataTable.Rows.Count == 0)
        {
            throw new AlbumSubscriptionNotFoundException(albumId);
        }
        DataRow datarow = dataTable.Rows[0];
        AlbumSubscriptionViewModel viewModel = datarow.ToAlbumSubscriptionViewModel();
        return viewModel;
    }

    public async Task<int?> GetAlbumVoteAsync(int albumId)
    {
        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string vote = await databaseAccessor.ScalarAsync<string>($"SELECT Vote FROM Subscriptions WHERE AlbumId={albumId} AND UserId={userId};");
        return string.IsNullOrEmpty(vote) ? null : Convert.ToInt32(vote);
    }

    public async Task VoteAlbumAsync(AlbumVoteInputModel inputModel)
    {
        if (inputModel.Vote < 1 || inputModel.Vote > 5)
        {
            throw new InvalidVoteException(inputModel.Vote);
        }
        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        FormattableString query = $"UPDATE Subscriptions SET Vote={inputModel.Vote} WHERE AlbumId={inputModel.Id} AND UserId={userId};";
        int updatedRows = await databaseAccessor.CommandAsync(query);
        if (updatedRows == 0)
        {
            throw new AlbumSubscriptionNotFoundException(inputModel.Id);
        }
    }
}
