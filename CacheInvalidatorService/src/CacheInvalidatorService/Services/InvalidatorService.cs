using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace CacheInvalidatorService.Services;

public class InvalidatorService
{
    private const string REDIS_PREFIX_INSTANCE = "AnimalAllies1_";
    
    private readonly HybridCache _hybridCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public InvalidatorService(
        HybridCache hybridCache,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _hybridCache = hybridCache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task InvalidateByKey(string key)
    {
        await _hybridCache.RemoveAsync(key);
            
        var fullKey = $"{REDIS_PREFIX_INSTANCE}{key}";
        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(fullKey, CommandFlags.FireAndForget);
    }
    
}