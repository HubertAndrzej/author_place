using AuthorPlace.Controllers;
using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.Services.Worker.Interfaces;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;
using AuthorPlace.Models.ViewModels.Albums;
using AuthorPlace.Models.ViewModels.Songs;

namespace AuthorPlace.Models.Services.Worker.Implemetations;

public class UserDataHostedService : BackgroundService, IUserDataService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IEmailClient emailClient;
    private readonly IServer server;
    private readonly IHostEnvironment environment;
    private readonly ILogger logger;
    private readonly LinkGenerator linkGenerator;
    private readonly BufferBlock<string> queue = new();

    public UserDataHostedService(IServiceScopeFactory serviceScopeFactory, IEmailClient emailClient, IServer server, IHostEnvironment environment, ILoggerFactory loggerFactory, LinkGenerator linkGenerator)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.emailClient = emailClient;
        this.server = server;
        this.environment = environment;
        logger = loggerFactory.CreateLogger("Workers");
        this.linkGenerator = linkGenerator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string? userId = null;
            try
            {
                userId = await queue.ReceiveAsync(stoppingToken);
                using IServiceScope serviceScope = serviceScopeFactory.CreateScope();
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                IAlbumService albumService = serviceProvider.GetRequiredService<IAlbumService>();
                ISongService songService = serviceProvider.GetRequiredService<ISongService>();
                UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                ApplicationUser user = await userManager.FindByIdAsync(userId);
                string zipFileUrl = await CreateZipFileAsync(userId, albumService, songService, stoppingToken);
                await SendZipFileLinkToUserAsync(user.Email, zipFileUrl, stoppingToken);
            }
            catch (Exception exception)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogError(exception, "An error occurred while preparing data for user {userId}", userId);
                }
            }
        }
    }

    private async Task<string> CreateZipFileAsync(string userId, IAlbumService albumService, ISongService songService, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        Guid zipFileId = Guid.NewGuid();
        string zipFilePath = GetUserDataZipFileLocation(userId, zipFileId);
        using FileStream file = File.OpenWrite(zipFilePath);
        using ZipArchive zip = new(file, ZipArchiveMode.Create);
        List<AlbumDetailViewModel> albums = await albumService.GetAlbumsByAuthorAsync(userId);
        foreach (AlbumDetailViewModel album in albums)
        {
            await AddZipEntry(zip, $"Albums/{album.Id}/Description.txt", $"{album.Title}\r\n{album.Description}", stoppingToken);
            using FileStream imageStream = File.OpenRead(Path.Combine(environment.ContentRootPath, "wwwroot", "Albums", $"{album.Id}.jpg"));
            await AddZipEntry(zip, $"Albums/{album.Id}/Image.jpg", imageStream, stoppingToken);
            foreach (SongViewModel songInAlbum in album.Songs)
            {
                SongDetailViewModel song = await songService.GetSongAsync(songInAlbum.Id);
                await AddZipEntry(zip, $"Albums/{album.Id}/Songs/{song.Id}.txt", $"{song.Title}\r\n{song.Description}", stoppingToken);
            }
        }
        IServerAddressesFeature? feature = server.Features.Get<IServerAddressesFeature>();
        Uri serverUri = new(feature!.Addresses.First());
        string? zipDownloadUrl = linkGenerator.GetUriByAction(action: nameof(UserDataController.Download), controller: "UserData", values: new { id = zipFileId }, scheme: serverUri.Scheme, host: new HostString(serverUri.Host, serverUri.Port));
        return zipDownloadUrl!;
    }

    private static async Task AddZipEntry(ZipArchive zip, string entryName, string? entryContent, CancellationToken stoppingToken)
    {
        ZipArchiveEntry entry = zip.CreateEntry(entryName, CompressionLevel.NoCompression);
        using Stream entryStream = entry.Open();
        using StreamWriter streamWriter = new(entryStream);
        await streamWriter.WriteAsync(entryContent.AsMemory(), stoppingToken);
    }

    private static async Task AddZipEntry(ZipArchive zip, string entryName, Stream entryContent, CancellationToken stoppingToken)
    {
        ZipArchiveEntry entry = zip.CreateEntry(entryName, CompressionLevel.NoCompression);
        using Stream entryStream = entry.Open();
        await entryContent.CopyToAsync(entryStream, stoppingToken);
    }

        public string GetUserDataZipFileLocation(string? userId, Guid zipFileId)
    {
        string zipFileName = $"{userId}_{zipFileId}.zip";
        string zipRootDirectoryPath = GetZipRootDirectoryPath();
        string zipFilePath = Path.Combine(zipRootDirectoryPath, zipFileName);
        return zipFilePath;
    }

    public IEnumerable<string> EnumerateAllUserDataZipFileLocations()
    {
        string zipRootDirectoryPath = GetZipRootDirectoryPath();
        return Directory.EnumerateFiles(zipRootDirectoryPath, "*.zip");
    }

    private string GetZipRootDirectoryPath()
    {
        return Path.Combine(environment.ContentRootPath, "Downloads");
    }

    private async Task SendZipFileLinkToUserAsync(string userEmail, string? zipFileUrl, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        await emailClient.SendEmailAsync(userEmail, null, "Your albums", $"The zip file containing the albums data is ready. You can download it from <a href=\"{zipFileUrl}\">{zipFileUrl}</a>", stoppingToken);
    }

    public void EnqueueUserDataDownload(string userId)
    {
        queue.Post(userId);
    }
}