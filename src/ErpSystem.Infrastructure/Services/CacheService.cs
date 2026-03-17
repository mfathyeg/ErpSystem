using System.Text.Json;
using ErpSystem.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace ErpSystem.Infrastructure.Services;

public sealed class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(cached))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(cached, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        var serialized = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(key, serialized, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Note: This requires Redis-specific implementation for production use
        // For basic IDistributedCache, pattern-based removal isn't supported
        // In production, use StackExchange.Redis directly for this operation
        return Task.CompletedTask;
    }
}
