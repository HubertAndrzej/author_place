using AuthorPlace.Models.Services.Worker.Interfaces;

namespace AuthorPlace.Models.Services.Worker.Implemetations;

public class ClearDataHostedService : BackgroundService
{
    private readonly IUserDataService userDataService;
    private readonly ILogger logger;

    public ClearDataHostedService(IUserDataService userDataService, ILoggerFactory loggerFactory)
    {
        this.userDataService = userDataService;
        logger = loggerFactory.CreateLogger("Workers");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                DateTime pastDate = DateTime.Now.AddDays(-3);
                foreach (string zipFile in userDataService.EnumerateAllUserDataZipFileLocations())
                {
                    FileInfo fileInfo = new(zipFile);
                    if (fileInfo.CreationTime < pastDate)
                    {
                        fileInfo.Delete();
                    }
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception exception)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogError(exception, "An error occurred while deleting old zip files");
                }
            }
        }
    }
}