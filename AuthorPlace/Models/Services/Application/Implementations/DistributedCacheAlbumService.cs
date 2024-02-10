using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.ViewModels;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace AuthorPlace.Models.Services.Application.Implementations;

public class DistributedCacheAlbumService : ICachedAlbumService
{
    private readonly IAlbumService albumService;
    private readonly IDistributedCache distributedCache;

    public DistributedCacheAlbumService(IAlbumService albumService, IDistributedCache distributedCache)
    {
        this.albumService = albumService;
        this.distributedCache = distributedCache;
    }

    public async Task<List<AlbumViewModel>> GetAlbumsAsync()
    {
        string key = $"Album";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            List<AlbumViewModel> albums = await albumService.GetAlbumsAsync();
            serializedObject = JsonConvert.SerializeObject(albums);
            await distributedCache.SetStringAsync(key, serializedObject);
            return albums;
        }
        else
        {
            return JsonConvert.DeserializeObject<List<AlbumViewModel>>(serializedObject)!;
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
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return album;
        }
        else
        {
            return JsonConvert.DeserializeObject<AlbumDetailViewModel>(serializedObject)!;
        }
    }
}
