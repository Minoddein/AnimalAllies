using Microsoft.Extensions.Caching.Hybrid;

namespace CacheInvalidatorService.Services;

public class InvalidatorService
{
    private readonly HybridCache _hybridCache;

    public InvalidatorService(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async Task InvalidateByKey(string key)
    {
        await _hybridCache.RemoveByTagAsync(key);
    }
}