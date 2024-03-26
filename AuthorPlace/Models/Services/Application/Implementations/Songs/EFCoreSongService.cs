using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.ViewModels.Songs;
using Microsoft.EntityFrameworkCore;

namespace AuthorPlace.Models.Services.Application.Implementations.Songs;

public class EFCoreSongService : ISongService
{
    private readonly AuthorPlaceDbContext dbContext;
    private readonly ILogger logger;

    public EFCoreSongService(AuthorPlaceDbContext dbContext, ILoggerFactory loggerFactory)
    {
        this.dbContext = dbContext;
        logger = loggerFactory.CreateLogger("Songs");
    }

    public SongDetailViewModel GetSong(int id)
    {
        IQueryable<SongDetailViewModel> queryLinq = dbContext.Songs!
            .AsNoTracking()
            .Where(song => song.Id == id)
            .Select(song => song.ToSongDetailViewModel());
        SongDetailViewModel? viewModel = queryLinq.FirstOrDefault();
        if (viewModel == null)
        {
            logger.LogWarning("Song {id} not found", id);
            throw new SongNotFoundException(id);
        }
        return viewModel;
    }

    public SongDetailViewModel CreateSong(SongCreateInputModel inputModel)
    {
        Song song = new(inputModel.Title!, inputModel.AlbumId);
        dbContext.Add(song);
        dbContext.SaveChanges();
        return song.ToSongDetailViewModel();
    }

    public SongUpdateInputModel GetSongForEditing(int id)
    {
        IQueryable<SongUpdateInputModel> queryLinq = dbContext.Songs!
            .AsNoTracking()
            .Where(song => song.Id == id)
            .Select(song => song.ToSongUpdateInputModel());
        SongUpdateInputModel? inputModel = queryLinq.FirstOrDefault();
        if (inputModel == null)
        {
            logger.LogWarning("Song {id} not found", id);
            throw new SongNotFoundException(id);
        }
        return inputModel;
    }

    public SongDetailViewModel UpdateSong(SongUpdateInputModel inputModel)
    {
        Song? song = dbContext.Songs!.Find(inputModel.Id);

        if (song == null)
        {
            logger.LogWarning("Song {inputModel.Id} not found", inputModel.Id);
            throw new SongNotFoundException(inputModel.Id);
        }
        song.ChangeTitle(inputModel.Title!);
        song.ChangeDescription(inputModel.Description!);
        song.ChangeDuration(inputModel.Duration);
        dbContext.Entry(song).Property(song => song.RowVersion).OriginalValue = inputModel.RowVersion;
        try
        {
            dbContext.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticConcurrencyException();
        }
        return song.ToSongDetailViewModel();
    }

    public void RemoveSong(SongDeleteInputModel inputModel)
    {
        Song? song = dbContext.Songs!.Find(inputModel.Id);
        if (song == null)
        {
            logger.LogWarning("Song {inputModel.Id} not found", inputModel.Id);
            throw new SongNotFoundException(inputModel.Id);
        }
        dbContext.Remove(song);
        dbContext.SaveChanges();
    }
}
