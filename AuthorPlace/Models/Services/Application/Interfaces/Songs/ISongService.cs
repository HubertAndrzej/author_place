using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.ViewModels.Songs;

namespace AuthorPlace.Models.Services.Application.Interfaces.Songs;

public interface ISongService
{
    public SongDetailViewModel GetSong(int id);
    public SongDetailViewModel CreateSong(SongCreateInputModel inputModel);
    public SongUpdateInputModel GetSongForEditing(int id);
    public SongDetailViewModel UpdateSong(SongUpdateInputModel inputModel);
    public void RemoveSong(SongDeleteInputModel inputModel);
}
