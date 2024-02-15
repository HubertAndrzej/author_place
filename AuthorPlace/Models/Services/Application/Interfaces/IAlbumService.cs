using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IAlbumService
{
    public Task<List<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model);
    public Task<AlbumDetailViewModel> GetAlbumAsync(int id);
}
