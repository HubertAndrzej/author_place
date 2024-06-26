﻿using AuthorPlace.Models.InputModels.Albums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.ViewModels.Albums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AuthorPlace.Models.Services.Application.Implementations.Albums;

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

    public Task<List<AlbumViewModel>> GetAlbumsByAuthorAsync(string authorId)
    {
        return albumService.GetAlbumsByAuthorAsync(authorId);
    }

    public Task<List<AlbumViewModel>> GetAlbumsBySubscriberAsync(string subscriberId)
    {
        return albumService.GetAlbumsBySubscriberAsync(subscriberId);
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
        memoryCache.Remove($"Album{inputModel.Id}");
        return viewModel;
    }

    public async Task DeleteAlbumAsync(AlbumDeleteInputModel inputModel)
    {
        await albumService.DeleteAlbumAsync(inputModel);
        memoryCache.Remove($"Album{inputModel.Id}");
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

    public Task<string> GetAlbumAuthorIdAsync(int albumId)
    {
        return memoryCache.GetOrCreateAsync($"AlbumAuthorId{albumId}", cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationOptions.CurrentValue.Duration));
            return albumService.GetAlbumAuthorIdAsync(albumId);
        });
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
