using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace CacheInvalidatorService.Services;

public class InvalidatorService
{
    private const string REDIS_PREFIX_INSTANCE = "AnimalAllies1_";
    private const string CACHE_CHANNEL = "cache_invalidator_channel";
    
    private readonly HybridCache _hybridCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ISubscriber _subscriber;

    public InvalidatorService(
        HybridCache hybridCache,
        IConnectionMultiplexer connectionMultiplexer,
        ISubscriber subscriber)
    {
        _hybridCache = hybridCache;
        _connectionMultiplexer = connectionMultiplexer;
        _subscriber = subscriber;
    }

    public async Task InvalidateByKey(string key)
    {
        await _hybridCache.RemoveAsync(key);
            
        var fullKey = $"{REDIS_PREFIX_INSTANCE}{key}";
        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(fullKey, CommandFlags.FireAndForget);
        
        await _subscriber.PublishAsync(CACHE_CHANNEL, key);
    }
    
}

