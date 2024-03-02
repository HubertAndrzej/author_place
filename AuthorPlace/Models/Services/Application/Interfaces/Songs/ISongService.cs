using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.ViewModels.Songs;

namespace AuthorPlace.Models.Services.Application.Interfaces.Songs;

public interface ISongService
{
    public Task<SongDetailViewModel> GetSongAsync(int id);
    public Task<SongDetailViewModel> CreateSongAsync(SongCreateInputModel inputModel);
    public Task<SongUpdateInputModel> GetSongForEditingAsync(int id);
    public Task<SongDetailViewModel> UpdateSongAsync(SongUpdateInputModel inputModel);
}
