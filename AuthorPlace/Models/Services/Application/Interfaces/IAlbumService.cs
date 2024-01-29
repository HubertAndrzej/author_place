using AuthorPlace.Models.ViewModels;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IAlbumService
{
    public List<AlbumViewModel> GetAlbums();
    public AlbumDetailViewModel GetAlbum(int id);
}
