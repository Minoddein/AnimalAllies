using AnimalAllies.SharedKernel.CachingConstants;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AnimalAllies.Core.BackgroundServices;

public class CacheInvalidatorBackgroundService: BackgroundService
{
    private readonly ISubscriber _subscriber;
    private readonly HybridCache _hybridCache;
    private readonly ILogger<CacheInvalidatorBackgroundService> _logger;

    public CacheInvalidatorBackgroundService(
        ISubscriber subscriber,
        HybridCache hybridCache,
        ILogger<CacheInvalidatorBackgroundService> logger)
    {
        _subscriber = subscriber;
        _hybridCache = hybridCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _subscriber.SubscribeAsync(CacheChannels.CACHE_CHANNEL, async void (channel, message) =>
        {
            try
            {
                var key = message.ToString();
                await _hybridCache.RemoveAsync(key, stoppingToken);
                await _hybridCache.RemoveByTagAsync(key, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Something went wrong with cache invalidator bg service: " + e.Message);
            }
        });
    }
}