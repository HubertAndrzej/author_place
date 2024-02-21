using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class DistributedCacheAlbumService : ICachedAlbumService
{
    private readonly IAlbumService albumService;
    private readonly IDistributedCache distributedCache;
    private readonly IOptionsMonitor<CacheDurationOptions> cacheDurationOptions;

    public DistributedCacheAlbumService(IAlbumService albumService, IDistributedCache distributedCache, IOptionsMonitor<CacheDurationOptions> cacheDurationOptions)
    {
        this.albumService = albumService;
        this.distributedCache = distributedCache;
        this.cacheDurationOptions = cacheDurationOptions;
    }

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model)
    {
        string key = $"Albums{model.Search}-{model.Page}-{model.OrderBy}-{model.Ascending}";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            ListViewModel<AlbumViewModel> albums = await albumService.GetAlbumsAsync(model);
            serializedObject = JsonConvert.SerializeObject(albums);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return albums;
        }
        else
        {
            return JsonConvert.DeserializeObject<ListViewModel<AlbumViewModel>>(serializedObject)!;
        }
    }

    public async Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        string key = $"Album{id}";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            AlbumDetailViewModel album = await albumService.GetAlbumAsync(id);
            serializedObject = JsonConvert.SerializeObject(album);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return album;
        }
        else
        {
            return JsonConvert.DeserializeObject<AlbumDetailViewModel>(serializedObject)!;
        }
    }

    public async Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync()
    {
        string key = $"BestRatingAlbums";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            List<AlbumViewModel> albums = await albumService.GetBestRatingAlbumsAsync();
            serializedObject = JsonConvert.SerializeObject(albums);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return albums;
        }
        else
        {
            return JsonConvert.DeserializeObject<List<AlbumViewModel>>(serializedObject)!;
        }
    }

    public async Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync()
    {
        string key = $"MostRecentAlbums";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            List<AlbumViewModel> albums = await albumService.GetMostRecentAlbumsAsync();
            serializedObject = JsonConvert.SerializeObject(albums);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return albums;
        }
        else
        {
            return JsonConvert.DeserializeObject<List<AlbumViewModel>>(serializedObject)!;
        }
    }

    public Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel)
    {
        return albumService.CreateAlbumAsync(inputModel);
    }

    public async Task<bool> IsAlbumUnique(string title, string author)
    {
        return await albumService.IsAlbumUnique(title, author);
    }
}
