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

    public async Task<SongDetailViewModel> GetSongAsync(int id)
    {
        IQueryable<SongDetailViewModel> queryLinq = dbContext.Songs!
            .AsNoTracking()
            .Where(song => song.Id == id)
            .Select(song => song.ToSongDetailViewModel());
        SongDetailViewModel? viewModel = await queryLinq
            .FirstOrDefaultAsync();
        if (viewModel == null)
        {
            logger.LogWarning("Song {id} not found", id);
            throw new SongNotFoundException(id);
        }
        return viewModel;
    }

    public async Task<SongDetailViewModel> CreateSongAsync(SongCreateInputModel inputModel)
    {
        Song song = new(inputModel.Title!, inputModel.AlbumId);
        dbContext.Add(song);
        await dbContext.SaveChangesAsync();
        return song.ToSongDetailViewModel();
    }

    public async Task<SongUpdateInputModel> GetSongForEditingAsync(int id)
    {
        IQueryable<SongUpdateInputModel> queryLinq = dbContext.Songs!
            .AsNoTracking()
            .Where(song => song.Id == id)
            .Select(song => song.ToSongUpdateInputModel());
        SongUpdateInputModel? inputModel = await queryLinq.FirstOrDefaultAsync();
        if (inputModel == null)
        {
            logger.LogWarning("Song {id} not found", id);
            throw new SongNotFoundException(id);
        }
        return inputModel;
    }

    public async Task<SongDetailViewModel> UpdateSongAsync(SongUpdateInputModel inputModel)
    {
        Song? song = await dbContext.Songs!.FindAsync(inputModel.Id);

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
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticConcurrencyException();
        }
        return song.ToSongDetailViewModel();
    }

    public async Task RemoveSongAsync(SongDeleteInputModel inputModel)
    {
        Song? song = await dbContext.Songs!.FindAsync(inputModel.Id);
        if (song == null)
        {
            logger.LogWarning("Song {inputModel.Id} not found", inputModel.Id);
            throw new SongNotFoundException(inputModel.Id);
        }
        dbContext.Remove(song);
        await dbContext.SaveChangesAsync();
    }
}
