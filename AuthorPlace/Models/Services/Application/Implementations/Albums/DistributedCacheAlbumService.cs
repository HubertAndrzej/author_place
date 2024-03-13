using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthorPlace.Models.Services.Application.Implementations.Albums;

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

    public async Task<ListViewModel<AlbumViewModel>> GetAlbumsAsync(AlbumListInputModel inputModel)
    {
        string key = $"Albums{inputModel.Search}-{inputModel.Page}-{inputModel.OrderBy}-{inputModel.Ascending}";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            ListViewModel<AlbumViewModel> albums = await albumService.GetAlbumsAsync(inputModel);
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
            AlbumDetailViewModel viewModel = await albumService.GetAlbumAsync(id);
            serializedObject = JsonConvert.SerializeObject(viewModel);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return viewModel;
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

    public Task<List<AlbumDetailViewModel>> GetAlbumsByAuthorAsync(string authorId)
    {
        return albumService.GetAlbumsByAuthorAsync(authorId);
    }

    public Task<AlbumDetailViewModel> CreateAlbumAsync(AlbumCreateInputModel inputModel)
    {
        return albumService.CreateAlbumAsync(inputModel);
    }

    public Task<AlbumUpdateInputModel> GetAlbumForEditingAsync(int id)
    {
        return albumService.GetAlbumForEditingAsync(id);
    }

    public async Task<AlbumDetailViewModel> UpdateAlbumAsync(AlbumUpdateInputModel inputModel)
    {
        AlbumDetailViewModel viewModel = await albumService.UpdateAlbumAsync(inputModel);
        await distributedCache.RemoveAsync($"Album{inputModel.Id}");
        return viewModel;
    }

    public async Task RemoveAlbumAsync(AlbumDeleteInputModel inputModel)
    {
        await albumService.RemoveAlbumAsync(inputModel);
        await distributedCache.RemoveAsync($"Album{inputModel.Id}");
    }

    public async Task<bool> IsAlbumUniqueAsync(string title, string authorId, int id)
    {
        return await albumService.IsAlbumUniqueAsync(title, authorId, id);
    }

    public async Task<string> GetAuthorAsync(int id)
    {
        return await albumService.GetAuthorAsync(id);
    }

    public Task SendQuestionToAlbumAuthorAsync(int id, string? question)
    {
        return albumService.SendQuestionToAlbumAuthorAsync(id, question);
    }

    public async Task<string> GetAlbumAuthorIdAsync(int albumId)
    {
        string key = $"AlbumAuthorId{albumId}";
        string serializedObject = await distributedCache.GetStringAsync(key);
        if (serializedObject == null)
        {
            string authorId = await albumService.GetAlbumAuthorIdAsync(albumId);
            serializedObject = JsonConvert.SerializeObject(authorId);
            DistributedCacheEntryOptions cacheOptions = new();
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            await distributedCache.SetStringAsync(key, serializedObject, cacheOptions);
            return authorId;
        }
        else
        {
            return JsonConvert.DeserializeObject<string>(serializedObject)!;
        }
    }

    public Task SubscribeAlbumAsync(AlbumSubscribeInputModel inputModel)
    {
        return albumService.SubscribeAlbumAsync(inputModel);
    }

    public Task<bool> IsAlbumSubscribedAsync(int albumId, string userId)
    {
        return albumService.IsAlbumSubscribedAsync(albumId, userId);
    }

    public Task<string> GetPaymentUrlAsync(int albumId)
    {
        return albumService.GetPaymentUrlAsync(albumId);
    }

    public Task<AlbumSubscribeInputModel> CapturePaymentAsync(int albumId, string token)
    {
        return albumService.CapturePaymentAsync(albumId, token);
    }

    public Task<AlbumSubscriptionViewModel> GetAlbumSubscriptionAsync(int albumId)
    {
        return albumService.GetAlbumSubscriptionAsync(albumId);
    }

    public Task<int?> GetAlbumVoteAsync(int albumId)
    {
        return albumService.GetAlbumVoteAsync(albumId);
    }

    public Task VoteAlbumAsync(AlbumVoteInputModel inputModel)
    {
        return albumService.VoteAlbumAsync(inputModel);
    }
}
