using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.ViewModels.Albums;

namespace AuthorPlace.Models.Services.Application.Interfaces.Albums;

public interface IAlbumService
{
    public Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel inputModel);
    public Task<AlbumDetailViewModel> GetAlbumAsync(int id);
    public Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync();
    public Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync();
    public Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel);
    public Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id);
    public Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel);
    public Task RemoveAlbumAsync(AlbumDeleteInputModel inputModel);
    public Task<bool> IsAlbumUniqueAsync(string title, string author, int id);
    public Task<string> GetAuthorAsync(int id);
    public Task SendQuestionToAlbumAuthorAsync(int id, string? question);
    public Task<string> GetAlbumAuthorIdAsync(int albumId);
}
