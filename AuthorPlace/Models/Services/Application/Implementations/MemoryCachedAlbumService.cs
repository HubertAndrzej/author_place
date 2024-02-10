using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class MemoryCachedAlbumService : ICachedAlbumService
{
    private readonly IAlbumService albumService;
    private readonly IMemoryCache memoryCache;

    public MemoryCachedAlbumService(IAlbumService albumService, IMemoryCache memoryCache)
    {
        this.albumService = albumService;
        this.memoryCache = memoryCache;
    }

    public Task<List<AlbumViewModel>> GetAlbumsAsync()
    {
        return memoryCache.GetOrCreateAsync($"Albums", cacheEntry =>
        {
            cacheEntry.SetSize(1);
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            return albumService.GetAlbumsAsync();
        });
    }

    public Task<AlbumDetailViewModel> GetAlbumAsync(int id)
    {
        return memoryCache.GetOrCreateAsync($"Album{id}", cacheEntry =>
        {
            cacheEntry.SetSize(1);
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            return albumService.GetAlbumAsync(id);
        });
    }
}
