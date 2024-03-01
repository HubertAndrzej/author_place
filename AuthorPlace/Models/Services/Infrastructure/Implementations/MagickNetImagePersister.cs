using AuthorPlace.Models.Exceptions.Infrastructure;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using ImageMagick;
using Microsoft.Extensions.Options;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class MagickNetImagePersister : IImagePersister
{
    private readonly IWebHostEnvironment environment;
    private readonly IOptionsMonitor<ImageSizeOptions> imageSizeOptions;
    private readonly SemaphoreSlim semaphore;

    public MagickNetImagePersister(IWebHostEnvironment environment, IOptionsMonitor<ImageSizeOptions> imageSizeOptions)
    {
        this.environment = environment;
        this.imageSizeOptions = imageSizeOptions;
        ResourceLimits.Height = 5000;
        ResourceLimits.Width = 5000;
        semaphore = new SemaphoreSlim(10);
    }

    public async Task<string> SaveAlbumImageAsync(int albumId, IFormFile formFile)
    {
        await semaphore.WaitAsync();
        try
        {
            string relativePath = $"/Albums/{albumId}.jpg";
            string internalPath = Path.Combine(environment.WebRootPath, "Albums", $"{albumId}.jpg");
            using Stream inputStream = formFile.OpenReadStream();
            using MagickImage image = new(inputStream);
            int width = imageSizeOptions.CurrentValue.Width;
            int height = imageSizeOptions.CurrentValue.Height;
            MagickGeometry magickGeometry = new(width, height)
            {
                FillArea = false
            };
            image.Resize(magickGeometry);
            image.Extent(width, height, Gravity.Center, MagickColor.FromRgb(224, 255, 255));
            image.Quality = 100;
            image.Write(internalPath, MagickFormat.Jpg);
            return relativePath;
        }
        catch (Exception exception)
        {
            throw new ImagePersistenceException(exception);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
