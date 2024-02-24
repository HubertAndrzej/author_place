using AuthorPlace.Models.Services.Infrastructure.Interfaces;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class InsecureImagePersister : IImagePersister
{
    private readonly IWebHostEnvironment environment;

    public InsecureImagePersister(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    public async Task<string> SaveAlbumImageAsync(int albumId, IFormFile formFile)
    {
        string relativePath = $"/Albums/{albumId}.jpg";
        string internalPath = Path.Combine(environment.WebRootPath, "Albums", $"{albumId}.jpg");
        using FileStream fileStream = File.OpenWrite(internalPath);
        await formFile.CopyToAsync(fileStream);
        return relativePath;
    }
}
