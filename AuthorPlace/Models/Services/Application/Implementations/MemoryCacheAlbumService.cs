using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.AspNetCore.Cors.Infrastructure;
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

    public Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel model)
    {
        return memoryCache.GetOrCreateAsync($"Albums{model.Search}-{model.Page}-{model.OrderBy}-{model.Ascending}", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return albumService.GetAlbumsAsync(model);
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

    public Task<List<AlbumViewModel>> GetBestRatingAlbumsAsync()
    {
        return memoryCache.GetOrCreateAsync($"BestRatingAlbums", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return albumService.GetBestRatingAlbumsAsync();
        });
    }

    public Task<List<AlbumViewModel>> GetMostRecentAlbumsAsync()
    {
        return memoryCache.GetOrCreateAsync($"MostRecentAlbums", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return albumService.GetMostRecentAlbumsAsync();
        });
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
