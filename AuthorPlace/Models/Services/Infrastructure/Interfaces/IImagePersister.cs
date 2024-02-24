namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IImagePersister
{
    public Task<string> SaveAlbumImageAsync(int albumId, IFormFile formFile);
}
