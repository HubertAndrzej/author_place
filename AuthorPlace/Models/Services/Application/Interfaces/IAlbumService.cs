using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IAlbumService
{
    public Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model);
    public Task<AlbumDetailViewModel> GetAlbumAsync(int id);
    public Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync();
    public Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync();
    public Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel);
    public Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id);
    public Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel);
    public Task<bool> IsAlbumUniqueAsync(string title, string author, int id);
    public Task<string> GetAuthorAsync(int id);
}
