using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IAlbumService
{
    public Task<List<AlbumViewModel>> GetAlbumsAsync(string? search, int page, string orderby, bool ascending);
    public Task<AlbumDetailViewModel> GetAlbumAsync(int id);
}
