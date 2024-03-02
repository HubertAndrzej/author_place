using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.ViewModels.Songs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthorPlace.Models.Services.Application.Implementations.Songs;

public class DistributedCacheSongService : ICachedSongService
{
    private readonly ISongService songService;
    private readonly IDistributedCache distributedCache;
    private readonly IOptionsMonitor<CacheDurationOptions> cacheDurationOptions;

    public DistributedCacheSongService(ISongService songService, IDistributedCache distributedCache, IOptionsMonitor<CacheDurationOptions> cacheDurationOptions)
    {
        this.songService = songService;
        this.distributedCache = distributedCache;
        this.cacheDurationOptions = cacheDurationOptions;
    }

    public async Task<SongDetailViewModel> GetSongAsync(int id)
    {
        string key = $"Song{id}";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            SongDetailViewModel viewModel = await songService.GetSongAsync(id);
            serializedObject = JsonConvert.SerializeObject(viewModel);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return viewModel;
        }
        else
        {
            return JsonConvert.DeserializeObject<SongDetailViewModel>(serializedObject)!;
        }
    }

    public Task<SongDetailViewModel> CreateSongAsync(SongCreateInputModel inputModel)
    {
        return songService.CreateSongAsync(inputModel);
    }

    public Task<SongUpdateInputModel> GetSongForEditingAsync(int id)
    {
        return songService.GetSongForEditingAsync(id);
    }

    public async Task<SongDetailViewModel> UpdateSongAsync(SongUpdateInputModel inputModel)
    {
        SongDetailViewModel viewModel = await songService.UpdateSongAsync(inputModel);
        await distributedCache.RemoveAsync($"Song{inputModel.Id}");
        return viewModel;
    }
}
