using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.ViewModels.Albums;
using AuthorPlace.Models.ViewModels.Songs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthorPlace.Models.Services.Application.Implementations.Songs;

public class MemoryCacheSongService : ICachedSongService
{
    private readonly ISongService songService;
    private readonly IMemoryCache memoryCache;
    private readonly IOptionsMonitor<CacheDurationOptions> cacheDurationOptions;

    public MemoryCacheSongService(ISongService songService, IMemoryCache memoryCache, IOptionsMonitor<CacheDurationOptions> cacheDurationOptions)
    {
        this.songService = songService;
        this.memoryCache = memoryCache;
        this.cacheDurationOptions = cacheDurationOptions;
    }

    public Task<SongDetailViewModel> GetSongAsync(int id)
    {
        return memoryCache.GetOrCreateAsync($"Song{id}", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return songService.GetSongAsync(id);
        });
    }

    public async Task<SongDetailViewModel> CreateSongAsync(SongCreateInputModel inputModel)
    {
        SongDetailViewModel viewModel = await songService.CreateSongAsync(inputModel);
        memoryCache.Remove($"Album{inputModel.AlbumId}");
        return viewModel;
    }

    public Task<SongUpdateInputModel> GetSongForEditingAsync(int id)
    {
        return songService.GetSongForEditingAsync(id);
    }

    public async Task<SongDetailViewModel> UpdateSongAsync(SongUpdateInputModel inputModel)
    {
        SongDetailViewModel viewModel = await songService.UpdateSongAsync(inputModel);
        memoryCache.Remove($"Album{inputModel.AlbumId}");
        memoryCache.Remove($"Song{inputModel.Id}");
        return viewModel;
    }

    public async Task RemoveSongAsync(SongDeleteInputModel inputModel)
    {
        await songService.RemoveSongAsync(inputModel);
        memoryCache.Remove($"Album{inputModel.AlbumId}");
        memoryCache.Remove($"Song{inputModel.Id}");
    }
}
