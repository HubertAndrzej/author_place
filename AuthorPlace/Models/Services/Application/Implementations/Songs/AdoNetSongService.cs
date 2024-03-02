using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ViewModels.Songs;
using System.Data;

namespace AuthorPlace.Models.Services.Application.Implementations.Songs;

public class AdoNetSongService : ISongService
{
    private readonly IDatabaseAccessor databaseAccessor;
    private readonly ILogger logger;

    public AdoNetSongService(IDatabaseAccessor databaseAccessor, ILoggerFactory loggerFactory)
    {
        this.databaseAccessor = databaseAccessor;
        logger = loggerFactory.CreateLogger("Songs");
    }

    public async Task<SongDetailViewModel> GetSongAsync(int id)
    {
        FormattableString songQuery = $"SELECT Id, CourseId, Title, Description, Duration FROM Lessons WHERE ID={id};";
        DataSet dataSet = await databaseAccessor.QueryAsync(songQuery);
        DataTable albumTable = dataSet.Tables[0];
        if (albumTable.Rows.Count != 1)
        {
            logger.LogWarning("Album {id} not found", id);
            throw new SongNotFoundException(id);
        }
        DataRow songRow = albumTable.Rows[0];
        SongDetailViewModel? viewModel = songRow.ToSongDetailViewModel();
        return viewModel;
    }

    public async Task<SongDetailViewModel> CreateSongAsync(SongCreateInputModel inputModel)
    {
        FormattableString insertQuery = $"INSERT INTO Songs (Title, AlbumId, Duration) VALUES ({inputModel.Title}, {inputModel.AlbumId}, '00:00:00');";
        await databaseAccessor.CommandAsync(insertQuery);
        FormattableString songQuery = $"SELECT last_insert_rowid();";
        int songId = await databaseAccessor.ScalarAsync<int>(songQuery);
        SongDetailViewModel? viewModel = await GetSongAsync(songId);
        return viewModel;
    }

    public async Task<SongUpdateInputModel> GetSongForEditingAsync(int id)
    {
        FormattableString query = $"SELECT Id, Title, Description, Duration, RowVersion FROM Songs WHERE Id={id}";
        DataSet dataSet = await databaseAccessor.QueryAsync(query);
        DataTable songTable = dataSet.Tables[0];
        if (songTable.Rows.Count != 1)
        {
            logger.LogWarning("Song {id} not found", id);
            throw new SongNotFoundException(id);
        }
        DataRow songRow = songTable.Rows[0];
        SongUpdateInputModel inputModel = songRow.ToSongUpdateInputModel();
        return inputModel;
    }

    public async Task<SongDetailViewModel> UpdateSongAsync(SongUpdateInputModel inputModel)
    {
        FormattableString updateQuery = $"UPDATE Songs SET Title={inputModel.Title}, Description={inputModel.Description}, Duration={inputModel.Duration:HH':'mm':'ss} WHERE Id={inputModel.Id} AND RowVersion={inputModel.RowVersion};";
        int affectedRows = await databaseAccessor.CommandAsync(updateQuery);
        if (affectedRows == 0)
        {
            FormattableString countQuery = $"SELECT COUNT(*) FROM Songs WHERE Id={inputModel.Id};";
            bool songExists = await databaseAccessor.ScalarAsync<bool>(countQuery);
            if (songExists)
            {
                logger.LogWarning("Update of song {inputModel.Id} failed", inputModel.Id);
                throw new OptimisticConcurrencyException();
            }
            else
            {
                logger.LogWarning("Song {inputModel.Id} not found", inputModel.Id);
                throw new SongNotFoundException(inputModel.Id);
            }
        }
        SongDetailViewModel? viewModel = await GetSongAsync(inputModel.Id);
        return viewModel;
    }
}
