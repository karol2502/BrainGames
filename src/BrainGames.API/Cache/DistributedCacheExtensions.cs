using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace BrainGames.API.Cache;

public static class DistributedCacheExtensions
{
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        var val = await cache.GetStringAsync(key, cancellationToken);
        return val is null ? default : JsonSerializer.Deserialize<T>(val);
    }
    
    public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken cancellationToken = default)
    {
        var cacheValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, cacheValue, cancellationToken);
    }
    
    public static async Task RemoveAsync(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }
}