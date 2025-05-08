using AnimalAllies.SharedKernel.CachingConstants;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AnimalAllies.Core.BackgroundServices;

public class CacheInvalidatorBackgroundService(
    ISubscriber subscriber,
    HybridCache hybridCache,
    ILogger<CacheInvalidatorBackgroundService> logger) : BackgroundService
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<CacheInvalidatorBackgroundService> _logger = logger;
    private readonly ISubscriber _subscriber = subscriber;

    [Obsolete]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _subscriber.SubscribeAsync(CacheChannels.CACHE_CHANNEL, async void (channel, message) =>
        {
            try
            {
                string[] value = message.ToString().Split("|");
                await _hybridCache.RemoveAsync(value, stoppingToken).ConfigureAwait(false);
                await _hybridCache.RemoveByTagAsync(value, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError("Something went wrong with cache invalidator bg service: " + e.Message);
            }
        }).ConfigureAwait(false);
}