using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IAlbumService
{
    public Task<List<AlbumViewModel>> GetAlbumsAsync(string? search);
    public Task<AlbumDetailViewModel> GetAlbumAsync(int id);
}
