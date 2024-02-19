using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IAlbumService
{
    public Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model);
    public Task<AlbumDetailViewModel> GetAlbumAsync(int id);
    public Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync();
    public Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync();
}
