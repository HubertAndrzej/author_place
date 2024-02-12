using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class MemoryCacheAlbumService : ICachedAlbumService
{
    private readonly IAlbumService albumService;
    private readonly IMemoryCache memoryCache;
    private readonly IOptionsMonitor<CacheDurationOptions> cacheDurationOptions;

    public MemoryCacheAlbumService(IAlbumService albumService, IMemoryCache memoryCache, IOptionsMonitor<CacheDurationOptions> cacheDurationOptions)
    {
        this.albumService = albumService;
        this.memoryCache = memoryCache;
        this.cacheDurationOptions = cacheDurationOptions;
    }

    public Task<List<AlbumViewModel>> GetAlbumsAsync(string? search)
    {
        return memoryCache.GetOrCreateAsync($"Albums{search}", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return albumService.GetAlbumsAsync(search);
        });
    }

    public Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        return memoryCache.GetOrCreateAsync($"Album{id}", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return albumService.GetAlbumAsync(id);
        });
    }
}
